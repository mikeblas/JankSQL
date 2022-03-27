namespace JankSQL.Engines
{
    using CSharpTest.Net.Collections;

    internal class IndexDefinition
    {
        private readonly string indexName;
        private readonly bool isUnique;
        private readonly List<(string columnName, bool isDescending, int heapColumnIndex)> columnInfos;
        private int nextUniquifer = 1;

        internal IndexDefinition(string indexName, bool isUnique, List<(string columnName, bool isDescending)> columnInfos, IEngineTable heap)
        {
            this.indexName = indexName;
            this.isUnique = isUnique;

            this.columnInfos = new List<(string columnName, bool isDescending, int heapColumnIndex)>();
            foreach (var columnInfo in columnInfos)
            {
                int idx = heap.ColumnIndex(columnInfo.columnName);
                this.columnInfos.Add((columnInfo.columnName, columnInfo.isDescending, idx));
            }
        }

        internal List<(string columnName, bool isDescending, int heapColumnIndex)> ColumnInfos
        {
            get { return columnInfos; }
        }

        internal bool IsUnique
        {
            get { return isUnique; }
        }

        internal string IndexName
        {
            get { return indexName; }
        }

        internal int AdvanceNextUniquifier()
        {
            return nextUniquifer++;
        }

        internal Tuple IndexKeyFromHeapRow(Tuple heapRow, BTreeTable heap)
        {
            Tuple indexKey;
            if (IsUnique)
            {
                indexKey = Tuple.CreateEmpty(ColumnInfos.Count);

                for (int i = 0; i < ColumnInfos.Count; i++)
                    indexKey.Values[i] = heapRow[ColumnInfos[i].heapColumnIndex];
            }
            else
            {
                // for a non-unique index, the key is as given plus a uniquifier integer
                indexKey = Tuple.CreateEmpty(ColumnInfos.Count + 1);
                for (int i = 0; i < ColumnInfos.Count; i++)
                    indexKey.Values[i] = heapRow[ColumnInfos[i].heapColumnIndex];

                indexKey[ColumnInfos.Count] = ExpressionOperand.IntegerFromInt(AdvanceNextUniquifier());
            }

            return indexKey;
        }

    }


    internal class BTreeTable : IEngineTable
    {
        private readonly List<FullColumnName> keyColumnNames;
        private readonly List<FullColumnName> valueColumnNames;
        private readonly Dictionary<string, int> columnNameIndexes;
        private readonly string tableName;
        private readonly bool hasUniqueKey;

        private readonly ExpressionOperandType[] keyTypes;
        private readonly ExpressionOperandType[] valueTypes;

        private readonly BPlusTree<Tuple, Tuple> myTree;

        private readonly Dictionary<string, (IndexDefinition def, BPlusTree<Tuple, Tuple> index)> indexes = new ();

        private int nextBookmark = 1;

        internal BTreeTable(string tableName, ExpressionOperandType[] keyTypes, List<FullColumnName> keyNames, ExpressionOperandType[] valueTypes, List<FullColumnName> valueNames)
        {
            myTree = new BPlusTree<Tuple, Tuple>(new IExpressionOperandComparer());
            hasUniqueKey = true;

            this.keyTypes = keyTypes;
            this.valueTypes = valueTypes;
            this.tableName = tableName;

            keyColumnNames = new ();
            valueColumnNames = new ();

            columnNameIndexes = new Dictionary<string, int>();
            int n = 0;
            for (int i = 0; i < valueNames.Count; i++)
            {
                FullColumnName fcn = FullColumnName.FromTableColumnName(tableName, valueNames[i].ColumnNameOnly());
                valueColumnNames.Add(fcn);
                columnNameIndexes.Add(fcn.ColumnNameOnly(), n++);
            }

            for (int i = 0; i < keyNames.Count; i++)
            {
                FullColumnName fcn = FullColumnName.FromTableColumnName(tableName, keyNames[i].ColumnNameOnly());
                keyColumnNames.Add(fcn);
                columnNameIndexes.Add(fcn.ColumnNameOnly(), n++);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BTreeTable"/> class as a heap.
        /// Creates a "heap" table with no unique index. Our approach to this is a table that has a fake
        /// "uniquifier" key as its bookmar_key. That single-column bookmark key maps to the values,
        /// which are all the columns given.
        /// </summary>
        /// <param name="tableName">string with the name of our table.</param>
        /// <param name="valueTypes">array containing the value types for each of our columns.</param>
        /// <param name="valueNames">array containing the names of each of the values in our columns.</param>
        internal BTreeTable(string tableName, ExpressionOperandType[] valueTypes, List<FullColumnName> valueNames)
        {
            myTree = new BPlusTree<Tuple, Tuple>(new IExpressionOperandComparer());
            hasUniqueKey = false;

            this.keyTypes = new ExpressionOperandType[] { ExpressionOperandType.INTEGER };
            this.valueTypes = valueTypes;
            this.tableName = tableName;

            keyColumnNames = new ();
            valueColumnNames = new ();

            columnNameIndexes = new Dictionary<string, int>();
            int n = 0;
            for (int i = 0; i < valueNames.Count; i++)
            {
                FullColumnName fcn = FullColumnName.FromTableColumnName(tableName, valueNames[i].ColumnNameOnly());
                valueColumnNames.Add(fcn);
                columnNameIndexes.Add(fcn.ColumnNameOnly(), n++);
            }

            // just the bookmark key
            FullColumnName fcnBookmark = FullColumnName.FromTableColumnName(tableName, "bookmark_key");
            keyColumnNames.Add(fcnBookmark);
            columnNameIndexes.Add(fcnBookmark.ColumnNameOnly(), n++);
        }


        public int ColumnCount
        {
            get
            {
                if (hasUniqueKey)
                    return keyColumnNames.Count + valueColumnNames.Count;
                else
                    return valueColumnNames.Count;
            }
        }

        public int ColumnIndex(string columnName)
        {
            FullColumnName probe = FullColumnName.FromColumnName(columnName);

            int index;
            if (columnNameIndexes.TryGetValue(probe.ColumnNameOnly(), out index))
                return index;
            else
                return -1;
        }

        public FullColumnName ColumnName(int n)
        {
            if (n < valueColumnNames.Count)
                return valueColumnNames[n];

            int m = n - valueColumnNames.Count;
            return keyColumnNames[m];
        }

        public int DeleteRows(List<ExpressionOperandBookmark> bookmarksToDelete)
        {
            if (bookmarksToDelete[0].Tuple.Length != keyColumnNames.Count)
                throw new ArgumentException($"bookmark key should be {keyColumnNames.Count} columns, received {bookmarksToDelete[0].Tuple.Length} columns");

            int deletedCount = 0;
            foreach (var bookmark in bookmarksToDelete)
            {
                if (myTree.ContainsKey(bookmark.Tuple))
                {
                    // delete it, but get the rest of the row first
                    Tuple heapRow = myTree[bookmark.Tuple];

                    bool found = myTree.Remove(bookmark.Tuple);
                    if (found)
                        deletedCount++;

                    // now, delete from the other indexes by building a key out of the value we know
                    foreach ((IndexDefinition indexDef, var indexTree) in indexes.Values)
                    {
                        Tuple indexKey = indexDef.IndexKeyFromHeapRow(heapRow, this);
                        indexTree.Remove(indexKey);
                    }
                }
            }

            return deletedCount;
        }

        public IEnumerator<RowWithBookmark> GetEnumerator()
        {
            return new BTreeRowEnumerator(myTree);
        }

        public void InsertRow(Tuple row)
        {
            Tuple heapKey;
            if (keyColumnNames[0].ColumnNameOnly().Equals("bookmark_key"))
            {
                // this is a heap with absolutely no index
                heapKey = Tuple.CreateEmpty(1);
                heapKey[0] = ExpressionOperand.IntegerFromInt(nextBookmark++);

                myTree.Add(heapKey, row);
            }
            else
            {
                heapKey = Tuple.CreateFromRange(row, 0, keyColumnNames.Count);
                Tuple value = Tuple.CreateFromRange(row, keyColumnNames.Count, valueColumnNames.Count);

                myTree.Add(heapKey, value);
            }

            // for each other index we maintain, insert there, too
            foreach ((IndexDefinition indexDef, var indexTree) in indexes.Values)
            {
                Tuple indexKey = indexDef.IndexKeyFromHeapRow(row, this);
                indexTree.Add(indexKey, heapKey);
            }

            if (indexes.Count > 0)
                Dump();
        }

        public void TruncateTable()
        {
            myTree.Clear();
        }

        public void Dump()
        {
            Console.WriteLine("===========");
            Console.WriteLine($"BTree Table {tableName}, hasUniqueKey == {hasUniqueKey}");
            Console.WriteLine($" Keys  : {string.Join(",", keyColumnNames)}");
            Console.WriteLine($" Values: {string.Join(",", valueColumnNames)}");
            foreach (var row in myTree)
                Console.WriteLine($"    {row.Key} ==> {row.Value}");

            foreach (var kv in indexes)
            {
                Console.WriteLine($" BTree Table {tableName}, index named {kv.Key}");
                foreach (var row in kv.Value.index)
                    Console.WriteLine($"     {row.Key} ==> {row.Value}");
            }
        }

        /// <summary>
        /// Adds a new index to this table.
        /// </summary>
        /// <param name="indexName">string with a name for the new index.</param>
        /// <param name="isUnique">boolean that's true if this new index is meant to be unique.</param>
        /// <param name="columnInfos">descriptions of the columns involved in this index</param>
        /// <exception cref="ExecutionException">Thrown if an error is encountered when building the index.</exception>
        internal void AddIndex(string indexName, bool isUnique, List<(string columnName, bool isDescending)> columnInfos)
        {
            if (indexes.ContainsKey(indexName))
                throw new ExecutionException($"Index definition {indexName} already exists");

            var indexTree = new BPlusTree<Tuple, Tuple>(new IExpressionOperandComparer());

            // enumerate and add
            using var e = myTree.GetEnumerator();
            IndexDefinition def = new (indexName, isUnique, columnInfos, this);

            while (e.MoveNext())
            {
                var row = e.Current;

                // the values are the key of the main index
                Tuple indexValue = Tuple.CreateEmpty(keyTypes.Length);
                for (int i = 0; i < keyTypes.Length; i++)
                    indexValue.Values[i] = row.Key[i];

                // the key depends on the index
                Tuple indexKey = def.IndexKeyFromHeapRow(row.Value, this);

                // and add it!
                indexTree.Add(indexKey, indexValue);
            }

            // add the new index definition and its corresponding tree
            indexes.Add(indexName, (def, indexTree));

            Dump();
        }
    }
}
