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
        private int nextBookmark = 1;

        private readonly BPlusTree<ExpressionOperand[], ExpressionOperand[]> myTree;

        internal BTreeTable(string tableName, ExpressionOperandType[] keyTypes, List<FullColumnName> keyNames, ExpressionOperandType[] valueTypes, List<FullColumnName> valueNames)
        {
            myTree = new BPlusTree<ExpressionOperand[], ExpressionOperand[]>(new IExpressionOperandComparer());
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
            myTree = new BPlusTree<ExpressionOperand[], ExpressionOperand[]>(new IExpressionOperandComparer());
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

        public void InsertRow(ExpressionOperand[] row)
        {
            if (keyColumnNames[0].ColumnNameOnly().Equals("bookmark_key"))
            {
                ExpressionOperand[] key = new ExpressionOperand[1];
                key[0] = ExpressionOperand.IntegerFromInt(nextBookmark++);

                myTree.Add(key, row);
            }
            else
            {
                ExpressionOperand[] key = new ExpressionOperand[keyColumnNames.Count];
                ExpressionOperand[] value = new ExpressionOperand[valueColumnNames.Count];

                Array.Copy(row, 0, key, 0, keyColumnNames.Count);
                Array.Copy(row, keyColumnNames.Count, value, 0, valueColumnNames.Count);

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
            foreach (var row in myTree)
            {
                Console.WriteLine($"    {string.Join(",", row.Key.Select(x => "[" + x + "]"))} ==> {string.Join(",", row.Value.Select(x => "[" + x + "]"))}");
            }
        }
    }
}
