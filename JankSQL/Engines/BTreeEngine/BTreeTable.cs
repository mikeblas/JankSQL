
using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;


namespace JankSQL.Engines
{
    class IExpressionOperandComparer : IComparer<ExpressionOperand[]>
    {
        int[]? keyOrder;

        public IExpressionOperandComparer(int[] keyOrder)
        {
            this.keyOrder = keyOrder;
        }

        public IExpressionOperandComparer()
        {
            keyOrder = null;
        }

        public int Compare(ExpressionOperand[]? x, ExpressionOperand[]? y)
        {
            if (x == null)
                throw new ArgumentNullException("x");
            if (y == null)
                throw new ArgumentNullException("y");
            if (x.Length != y.Length)
                throw new ArgumentException($"sizes are different: {x.Length} and {y.Length}");

            int ret;
            if (keyOrder != null)
            {
                int keyNumber = 0;
                do
                {
                    ret = x[keyOrder[keyNumber]].CompareTo(y[keyOrder[keyNumber]]);
                    keyNumber++;
                } while (ret == 0 && keyNumber < keyOrder.Length);
            }
            else
            {
                int keyNumber = 0;
                do
                {
                    ret = x[keyNumber].CompareTo(y[keyNumber]);
                    keyNumber++;
                } while (ret == 0 && keyNumber < x.Length);
            }

            // Console.WriteLine($"{String.Join(",", x)} compared to {String.Join(",", y)} --> {ret}");

            return ret;
        }
    }


    internal class BTreeTable : IEngineTable
    {
        List<FullColumnName> keyColumnNames;
        List<FullColumnName> valueColumnNames;
        Dictionary<FullColumnName, int> columnNameIndexes;
        ExpressionOperandType[] keyTypes;
        ExpressionOperandType[] valueTypes;
        int nextBookmark = 1;

        BPlusTree<ExpressionOperand[], ExpressionOperand[]> myTree;

        internal BTreeTable(ExpressionOperandType[] keyTypes, List<FullColumnName> keyNames, ExpressionOperandType[] valueTypes, List<FullColumnName> valueNames)
        {
            myTree = new BPlusTree<ExpressionOperand[], ExpressionOperand[]>(new IExpressionOperandComparer());

            keyColumnNames = keyNames;
            valueColumnNames = valueNames;

            this.keyTypes = keyTypes;
            this.valueTypes = valueTypes;

            columnNameIndexes = new Dictionary<FullColumnName, int>();
            int n = 0;
            for (int i = 0; i < keyColumnNames.Count; i++)
                columnNameIndexes.Add(keyColumnNames[i], n++);
            for (int i = 0; i < valueColumnNames.Count; i++)
                columnNameIndexes.Add(valueColumnNames[i], n++);
        }

        public int RowCount => throw new NotImplementedException();

        public int ColumnCount => keyColumnNames.Count + valueColumnNames.Count;

        public int ColumnIndex(string columnName)
        {
            FullColumnName probe = FullColumnName.FromColumnName(columnName);

            int index;
            if (columnNameIndexes.TryGetValue(probe, out index))
                return index;
            else
                return -1;
        }

        public FullColumnName ColumnName(int n)
        {
            if (n < keyColumnNames.Count)
                return keyColumnNames[n];

            int m = n - keyColumnNames.Count;
            return valueColumnNames[m];
        }

        public int DeleteRows(List<int> rowIndexesToDelete)
        {
            throw new NotImplementedException();
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

        public ExpressionOperand[] Row(int n)
        {

            throw new NotImplementedException();
        }

        public void TruncateTable()
        {
            myTree.Clear();
        }
    }
}
