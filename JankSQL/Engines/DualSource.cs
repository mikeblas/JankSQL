namespace JankSQL.Engines
{
    public class DualSource : IEngineSource
    {
        int IEngineSource.RowCount => 1;

        int IEngineSource.ColumnCount => 0;

        int IEngineSource.ColumnIndex(string columnName)
        {
            return -1;
        }

        FullColumnName IEngineSource.ColumnName(int n)
        {
            throw new NotImplementedException();
        }

        ExpressionOperand[] IEngineSource.Row(int n)
        {
            ExpressionOperand[] ret = Array.Empty<ExpressionOperand>();
            return ret;
        }

        void IEngineSource.Load()
        {
        }

        public void TruncateTable()
        {
            throw new NotImplementedException();
        }
    }
}

