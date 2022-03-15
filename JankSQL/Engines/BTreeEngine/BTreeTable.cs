
using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using System.Collections;

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


    internal class BTreeRowEnumerator : IEnumerator<ExpressionOperand[]>
    {
        IEnumerator<KeyValuePair<ExpressionOperand[], ExpressionOperand[]>> treeEnumerator;

        internal BTreeRowEnumerator(BPlusTree<ExpressionOperand[], ExpressionOperand[]> tree)
        {
            treeEnumerator = tree.GetEnumerator();
        }

        public ExpressionOperand[] Current
        {
            get
            {
                ExpressionOperand[] result = new ExpressionOperand[treeEnumerator.Current.Value.Length + 1];

                for (int i = 0; i < treeEnumerator.Current.Value.Length; i++)
                    result[i] = treeEnumerator.Current.Value[i];

                result[treeEnumerator.Current.Value.Length] = new ExpressionOperandBookmark(treeEnumerator.Current.Key);
                return result;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose()
        {
            treeEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            return treeEnumerator.MoveNext();
        }

        public void Reset()
        {
            treeEnumerator.Reset();
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
        string tableName;

        BPlusTree<ExpressionOperand[], ExpressionOperand[]> myTree;

        internal BTreeTable(string tableName, ExpressionOperandType[] keyTypes, List<FullColumnName> keyNames, ExpressionOperandType[] valueTypes, List<FullColumnName> valueNames)
        {
            myTree = new BPlusTree<ExpressionOperand[], ExpressionOperand[]>(new IExpressionOperandComparer());

            this.keyTypes = keyTypes;
            this.valueTypes = valueTypes;
            this.tableName = tableName;

            keyColumnNames = new();
            valueColumnNames = new();

            columnNameIndexes = new Dictionary<FullColumnName, int>();
            int n = 0;
            for (int i = 0; i < valueNames.Count; i++)
            {
                FullColumnName fcn = FullColumnName.FromTableColumnName(tableName, valueNames[i].ColumnNameOnly());
                valueColumnNames.Add(fcn);
                columnNameIndexes.Add(fcn, n++);
            }
            for (int i = 0; i < keyNames.Count; i++)
            {
                FullColumnName fcn = FullColumnName.FromTableColumnName(tableName, keyNames[i].ColumnNameOnly());
                keyColumnNames.Add(fcn);
                columnNameIndexes.Add(fcn, n++);
            }
        }

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
            if (n < valueColumnNames.Count)
                return valueColumnNames[n];

            int m = n - valueColumnNames.Count;
            return keyColumnNames[m];
        }

        public int DeleteRows(List<int> rowIndexesToDelete)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ExpressionOperand[]> GetEnumerator()
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
    }
}
