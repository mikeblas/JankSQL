namespace JankSQL.Engines
{
    public class DualSource : IEngineTable
    {
        public int ColumnCount => 0;

        public int ColumnIndex(string columnName)
        {
            return -1;
        }

        public FullColumnName ColumnName(int n)
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

        public IEnumerator<RowWithBookmark> GetEnumerator()
        {
            List<RowWithBookmark> xl = new();
            
            ExpressionOperand[] x = Array.Empty<ExpressionOperand>();
            ExpressionOperandBookmark bm = new ExpressionOperandBookmark(Array.Empty<ExpressionOperand>());

            xl.Add(new RowWithBookmark(x, bm));

            return xl.GetEnumerator();
        }
    }
}

