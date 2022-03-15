namespace JankSQL.Engines
{
    public class DualSource : IEngineTable
    {
        public int ColumnCount => 0;

        int IEngineTable.ColumnIndex(string columnName)
        {
            return -1;
        }

        FullColumnName IEngineTable.ColumnName(int n)
        {
            throw new NotImplementedException();
        }

        void Load()
        {
        }

        public void TruncateTable()
        {
            throw new NotImplementedException();
        }

        public void InsertRow(ExpressionOperand[] row)
        {
            throw new NotImplementedException();
        }

        public int DeleteRows(List<ExpressionOperandBookmark> bookmarksToDelete)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ExpressionOperand[]> GetEnumerator()
        {
            List<ExpressionOperand[]> xl = new();
            ExpressionOperand[] x = Array.Empty<ExpressionOperand>();
            xl.Add(x);

            return xl.GetEnumerator();
        }
    }
}

