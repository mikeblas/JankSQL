namespace JankSQL
{
    public class ExecuteResult
    {
        private ResultSet? resultSet;
        private int rowsAffected;

        private ExecuteResult()
        {
            this.ExecuteStatus = ExecuteStatus.NOT_EXECUTED;
        }

        internal static ExecuteResult SuccessWithRowsAffected(int rowsaffected)
        {
            ExecuteResult result = new ();
            result.ExecuteStatus = ExecuteStatus.SUCCESSFUL;
            result.rowsAffected = rowsaffected;

            return result;
        }

        internal static ExecuteResult SuccessWithMessage(string message)
        {
            ExecuteResult result = new ();
            result.ExecuteStatus = ExecuteStatus.SUCCESSFUL_WITH_MESSAGE;
            result.ErrorMessage = message;
            return result;
        }

        internal static ExecuteResult SuccessWithResultSet(ResultSet resultSet)
        {
            ExecuteResult result = new ();
            result.ExecuteStatus = ExecuteStatus.SUCCESSFUL;
            result.resultSet = resultSet;

            return result;
        }

        internal static ExecuteResult FailureWithError(string message)
        {
            ExecuteResult result = new ();
            result.ErrorMessage = message;
            result.ExecuteStatus = ExecuteStatus.FAILED;

            return result;
        }

        public ResultSet ResultSet
        {
            get
            {
                if (resultSet == null)
                    throw new InvalidOperationException("no rowset is available");
                return resultSet;
            }

            set
            {
                resultSet = value;
            }
        }

        public ExecuteStatus ExecuteStatus { get; set; }

        public string? ErrorMessage { get; set; }

        public int RowsAffected { get { return rowsAffected; } }
    }
}
