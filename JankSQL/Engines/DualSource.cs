namespace JankSQL.Engines
{
    public class DualSource : IEngineSource
    {
        public int RowCount => 1;

        public int ColumnCount => 0;

        public int ColumnIndex(string columnName)
        {
            return -1;
        }

        public FullColumnName ColumnName(int n)
        {
            return null;
        }

        public ExpressionOperand[] Row(int n)
        {
            ExpressionOperand[] ret = new ExpressionOperand[0];
            return ret;
        }

        public void Load()
        {
        }
    }
}

