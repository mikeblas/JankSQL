namespace JankSQL.Engines
{
    using System.Collections;
    using CSharpTest.Net;
    using CSharpTest.Net.Collections;

    using JankSQL.Expressions;


    internal class BTreeTable : IEngineTable, IEnumerable, IEnumerable<RowWithBookmark>, IDisposable
    {
        private readonly FullColumnName[] keyColumnNames;
        private readonly FullColumnName[] valueColumnNames;
        private readonly Dictionary<string, int> columnNameIndexes;
        private readonly string tableName;
        private readonly bool hasUniqueKey;

        private readonly ExpressionOperandType[] keyTypes;
        private readonly ExpressionOperandType[] valueTypes;

        // the BTree that implments this table
        private readonly BPlusTree<Tuple, Tuple> myTree;

        // a dictionary of indexes to BTree objects for each index
        private readonly Dictionary<string, (IndexDefinition def, BPlusTree<Tuple, Tuple> index)> indexes = new ();

        // next bookmark ID this table will use
        private int nextBookmark = 1337;

        // were we disposed?
        private bool isDisposed = false;

        internal BTreeTable(string tableName, ExpressionOperandType[] keyTypes, IEnumerable<FullColumnName> keyNames, ExpressionOperandType[] valueTypes, IEnumerable<FullColumnName> valueNames, BPlusTree<Tuple, Tuple>.OptionsV2? options)
        {
            if (keyTypes.Length != keyNames.Count())
                throw new ArgumentException("keyTypes length doesn't match keyNames length");
            if (valueTypes.Length != valueNames.Count())
                throw new ArgumentException("valueTypes length doesn't match valueNames length");

            if (options == null)
            {
                myTree = new BPlusTree<Tuple, Tuple>(new IExpressionOperandComparer());
            }
            else
            {
                options.KeyComparer = new IExpressionOperandComparer();
                myTree = new BPlusTree<Tuple, Tuple>(options);
            }

            hasUniqueKey = true;

            this.keyTypes = keyTypes;
            this.valueTypes = valueTypes;
            this.tableName = tableName;

            keyColumnNames = new FullColumnName[keyNames.Count()];
            valueColumnNames = new FullColumnName[valueNames.Count()];

            columnNameIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            int n = 0;
            int keyIndex = 0;
            int valueIndex = 0;
            foreach (var valueName in valueNames)
            {
                FullColumnName fcn = FullColumnName.FromTableColumnName(tableName, valueName.ColumnNameOnly());
                columnNameIndexes.Add(fcn.ColumnNameOnly(), n++);
                valueColumnNames[valueIndex++] = fcn;
            }

            foreach (var keyName in keyNames)
            {
                FullColumnName fcn = FullColumnName.FromTableColumnName(tableName, keyName.ColumnNameOnly());
                columnNameIndexes.Add(fcn.ColumnNameOnly(), n++);
                keyColumnNames[keyIndex++] = fcn;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BTreeTable"/> class as a heap.
        /// Creates a "heap" table with no unique index. Our approach to this is a table that has a fake
        /// "uniquifier" key as its bookmark_key. That single-column bookmark key maps to the values,
        /// which are all the columns given.
        /// </summary>
        /// <param name="tableName">string with the name of our table.</param>
        /// <param name="valueTypes">array containing the value types for each of our columns.</param>
        /// <param name="valueNames">list containing the names of each of the values in our columns.</param>
        internal BTreeTable(string tableName, ExpressionOperandType[] valueTypes, IEnumerable<FullColumnName> valueNames, BPlusTree<Tuple, Tuple>.OptionsV2? options)
        {
            int countValueNames = valueNames.Count();
            if (valueTypes.Length != countValueNames)
                throw new ArgumentException("valueTypes length doesn't match valueNames length");

            if (options == null)
            {
                myTree = new BPlusTree<Tuple, Tuple>(new IExpressionOperandComparer());
            }
            else
            {
                options.KeyComparer = new IExpressionOperandComparer();
                myTree = new BPlusTree<Tuple, Tuple>(options);
            }

            hasUniqueKey = false;

            this.keyTypes = new ExpressionOperandType[] { ExpressionOperandType.INTEGER };
            this.valueTypes = valueTypes;
            this.tableName = tableName;

            valueColumnNames = new FullColumnName[countValueNames];

            columnNameIndexes = new Dictionary<string, int>();
            int n = 0;
            foreach (var valueName in valueNames)
            {
                FullColumnName fcn = FullColumnName.FromTableColumnName(tableName, valueName.ColumnNameOnly());
                valueColumnNames[n] = fcn;
                columnNameIndexes.Add(fcn.ColumnNameOnly(), n++);
            }

            // just the bookmark key
            FullColumnName fcnBookmark = FullColumnName.FromTableColumnName(tableName, "bookmark_key");
            keyColumnNames = new FullColumnName[] { fcnBookmark };
            columnNameIndexes.Add(fcnBookmark.ColumnNameOnly(), n++);
        }

        public int ColumnCount
        {
            get
            {
                if (hasUniqueKey)
                    return keyColumnNames.Length + valueColumnNames.Length;
                else
                    return valueColumnNames.Length;
            }
        }

        public int ColumnIndex(string columnName)
        {
            FullColumnName probe = FullColumnName.FromColumnName(columnName);

            if (columnNameIndexes.TryGetValue(probe.ColumnNameOnly(), out int index))
                return index;
            else
                return -1;
        }

        public FullColumnName ColumnName(int n)
        {
            if (n < valueColumnNames.Length)
                return valueColumnNames[n];

            int m = n - valueColumnNames.Length;
            return keyColumnNames[m];
        }

        public IndexAccessor? Index(string indexName)
        {
            if (!indexes.TryGetValue(indexName, out (IndexDefinition def, BPlusTree<Tuple, Tuple> index) o))
                return null;

            foreach (var row in o.index)
            {
                Console.WriteLine($"RawIndex: {row.Key}: {row.Value}");
            }

            return new IndexAccessor(o.def, o.index);
        }

        public int DeleteRows(List<ExpressionOperandBookmark> bookmarksToDelete)
        {
            // nothing to delete?
            if (bookmarksToDelete.Count == 0)
                return 0;

            if (bookmarksToDelete[0].Tuple.Length != keyColumnNames.Length)
                throw new ArgumentException($"bookmark key should be {keyColumnNames.Length} columns, received {bookmarksToDelete[0].Tuple.Length} columns");

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
            return new BTreeHeapRowEnumerator(myTree);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<RowWithBookmark> GetIndexEnumerator(string indexName)
        {
            if (!indexes.TryGetValue(indexName, out (IndexDefinition def, BPlusTree<Tuple, Tuple> index) o))
                throw new ArgumentException($"No index named {indexName} exists");

            return new BTreeIndexRowEnumerator(o.index, o.def);
        }

        public void InsertRow(Tuple row)
        {
            if (row.Length != this.ColumnCount)
                throw new ArgumentException($"got {row.Length}, expected {this.ColumnCount}");
            //TODO: check types

            Tuple heapKey;
            if (keyColumnNames[0].ColumnNameOnly().Equals("bookmark_key"))
            {
                // this is a heap with absolutely no index
                heapKey = Tuple.FromSingleValue(nextBookmark++);
                myTree.Add(heapKey, row);
            }
            else
            {
                heapKey = Tuple.CreateFromRange(row, 0, keyColumnNames.Length);
                Tuple value = Tuple.CreateFromRange(row, keyColumnNames.Length, valueColumnNames.Length);

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

        public void Commit()
        {
            myTree.Commit();
        }

        public void Rollback()
        {
            myTree.Rollback();
        }

        public string? BestIndex(IEnumerable<(string columnName, bool isEquality)> accessColumns)
        {
            double bestEqualityScore = -1.0;
            double bestInequalityScore = -1.0;
            string? bestIndex = null;

            Dictionary<string, bool> columns = new (StringComparer.OrdinalIgnoreCase);
            foreach (var accessColumn in accessColumns)
                columns.Add(accessColumn.columnName, accessColumn.isEquality);

            // for each index
            foreach ((var k, var v) in indexes)
            {
                double equalityScore = -1.0;
                double inequalityScore = -1.0;
                bool matchingEqualities = true;

                // for each column in that index
                foreach (var columnInfo in v.def.ColumnInfos)
                {
                    // is that in the columns we're considering here?
                    if (!columns.TryGetValue(columnInfo.columnName, out bool isEquality))
                    {
                        // not a match. we're done
                        break;
                    }

                    // if this is an equality requirement, and we're done matching equalities,
                    // we're also as good as done
                    if (!matchingEqualities && isEquality)
                        break;

                    // if this is an inequality requirement and we were matching inequalities,
                    // we transition to ineqialities now.
                    if (matchingEqualities && !isEquality)
                        matchingEqualities = false;

                    if (matchingEqualities)
                    {
                        if (equalityScore == -1.0)
                            equalityScore = 0.0;
                        equalityScore += 1.0 / columns.Count;
                    }
                    else
                    {
                        if (inequalityScore == -1.0)
                            inequalityScore = 0.0;
                        inequalityScore += 1.0 / columns.Count;
                    }
                }

                Console.WriteLine($"index {k} has score {equalityScore}, {inequalityScore}");
                if (equalityScore > bestEqualityScore || (equalityScore == bestEqualityScore && inequalityScore > bestInequalityScore))
                {
                    bestEqualityScore = equalityScore;
                    bestInequalityScore = inequalityScore;
                    bestIndex = k;
                }
            }

            return bestIndex;
        }

        public void Dump()
        {
            Console.WriteLine("===========");
            Console.WriteLine($"BTree Table {tableName}, hasUniqueKey == {hasUniqueKey}");
            Console.WriteLine($" Keys  : {string.Join(",", (object[])keyColumnNames)}");
            Console.WriteLine($" Values: {string.Join(",", (object[])valueColumnNames)}");
            foreach (var row in myTree)
                Console.WriteLine($"    {row.Key} ==> {row.Value}");

            foreach (var kv in indexes)
            {
                Console.WriteLine($" BTree Table {tableName}, index named {kv.Key}");
                foreach (var row in kv.Value.index)
                    Console.WriteLine($"     {row.Key} ==> {row.Value}");
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                try
                {
                    foreach ((IndexDefinition _, var indexTree) in indexes.Values)
                        indexTree.Dispose();
                    myTree.Dispose();
                }
                finally
                {
                    isDisposed = true;
                }
            }
        }

        /// <summary>
        /// Adds a new index to this table.
        /// </summary>
        /// <param name="indexName">string with a name for the new index.</param>
        /// <param name="isUnique">boolean that's true if this new index is meant to be unique.</param>
        /// <param name="columnInfos">descriptions of the columns involved in this index.</param>
        /// <exception cref="ExecutionException">Thrown if an error is encountered when building the index.</exception>
        internal void AddIndex(string indexName, bool isUnique, IEnumerable<(string columnName, bool isDescending)> columnInfos, BPlusTree<Tuple, Tuple>.OptionsV2? options)
        {
            if (indexes.ContainsKey(indexName))
                throw new ExecutionException($"Index definition {indexName} already exists");

            BPlusTree<Tuple, Tuple> indexTree;

            if (options == null)
            {
                indexTree = new BPlusTree<Tuple, Tuple>(new IExpressionOperandComparer(columnInfos.Select(x => x.isDescending).ToArray()));
            }
            else
            {
                options.KeyComparer = new IExpressionOperandComparer(columnInfos.Select(x => x.isDescending).ToArray());
                indexTree = new BPlusTree<Tuple, Tuple>(options);
            }

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
                try
                {
                    indexTree.Add(indexKey, indexValue);
                }
                catch (DuplicateKeyException)
                {
                    indexTree.Dispose();
                    throw new ExecutionException($"Duplicate key found when building unique index: {indexKey}");
                }
            }

            // add the new index definition and its corresponding tree
            indexes.Add(indexName, (def, indexTree));

            Dump();
        }
    }
}
