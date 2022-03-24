namespace JankSQL.Engines
{
    using CSharpTest.Net.Collections;

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
            {
                throw new ArgumentException($"bookmark key should be {keyColumnNames.Count} columns, received {bookmarksToDelete[0].Tuple.Length} columns");
            }

            int deletedCount = 0;
            foreach (var bookmark in bookmarksToDelete)
            {
                bool found = myTree.Remove(bookmark.Tuple);
                if (found) deletedCount++;
            }

            return deletedCount;
        }

        public IEnumerator<RowWithBookmark> GetEnumerator()
        {
            return new BTreeRowEnumerator(myTree);
        }

        public void InsertRow(Tuple row)
        {
            if (keyColumnNames[0].ColumnNameOnly().Equals("bookmark_key"))
            {
                Tuple key = Tuple.CreateEmpty(1); ;
                key[0] = ExpressionOperand.IntegerFromInt(nextBookmark++);

                myTree.Add(key, row);
            }
            else
            {
                Tuple key = Tuple.CreateFromRange(row, 0, keyColumnNames.Count);
                Tuple value = Tuple.CreateFromRange(row, keyColumnNames.Count, valueColumnNames.Count);

                myTree.Add(key, value);
            }
        }

        public void TruncateTable()
        {
            myTree.Clear();
        }

        public void Dump()
        {
            Console.WriteLine($"BTree Table {tableName}, hasUniqueKey == {hasUniqueKey}:");
            Console.WriteLine($" Keys  : {string.Join(",", keyColumnNames)}");
            Console.WriteLine($" Values: {string.Join(",", valueColumnNames)}");
            foreach (var row in myTree)
                Console.WriteLine($"    {row.Key} ==> {row.Value}");
        }
    }
}
