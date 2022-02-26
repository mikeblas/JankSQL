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
            return null;
        }

        ExpressionOperand[] IEngineSource.Row(int n)
        {
            ExpressionOperand[] ret = new ExpressionOperand[0];
            return ret;
        }

        void IEngineSource.Load()
        {
        }
    }
}

