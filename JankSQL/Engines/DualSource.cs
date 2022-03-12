namespace JankSQL.Engines
{
    public class DualSource : IEngineTable
    {
        int IEngineTable.RowCount => 1;

        int IEngineTable.ColumnCount => 0;

        int IEngineTable.ColumnIndex(string columnName)
        {
            return -1;
        }

        FullColumnName IEngineTable.ColumnName(int n)
        {
            throw new NotImplementedException();
        }

        ExpressionOperand[] IEngineTable.Row(int n)
        {
            ExpressionOperand[] ret = Array.Empty<ExpressionOperand>();
            return ret;
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

        public int DeleteRows(List<int> rowIndexesToDelete)
        {
            throw new NotImplementedException();
        }
    }
}

