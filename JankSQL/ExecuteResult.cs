namespace JankSQL
{
    public sealed class ExecuteResult
    {
        private ResultSet? resultSet;
        private int rowsAffected;

        private ExecuteResult()
        {
            this.ExecuteStatus = ExecuteStatus.NOT_EXECUTED;
        }

        public ResultSet ResultSet
        {
            get
            {
                if (resultSet == null)
                    throw new InvalidOperationException("no row set is available");
                return resultSet;
            }

            set
            {
                resultSet = value;
            }
        }

        public ExecuteStatus ExecuteStatus { get; set; }

        public string? ErrorMessage { get; set; }

        public int RowsAffected
        {
            get
            {
                return rowsAffected;
            }
        }

        internal static ExecuteResult SuccessWithRowsAffected(int rowsaffected)
        {
            ExecuteResult result = new()
            {
                ExecuteStatus = ExecuteStatus.SUCCESSFUL,
                rowsAffected = rowsaffected
            };

            return result;
        }

        internal static ExecuteResult SuccessWithMessage(string message)
        {
            ExecuteResult result = new()
            {
                ExecuteStatus = ExecuteStatus.SUCCESSFUL_WITH_MESSAGE,
                ErrorMessage = message
            };
            return result;
        }

        internal static ExecuteResult SuccessWithResultSet(ResultSet resultSet)
        {
            ExecuteResult result = new()
            {
                ExecuteStatus = ExecuteStatus.SUCCESSFUL,
                resultSet = resultSet
            };

            return result;
        }

        internal static ExecuteResult FailureWithError(string message)
        {
            ExecuteResult result = new()
            {
                ErrorMessage = message,
                ExecuteStatus = ExecuteStatus.FAILED
            };

            return result;
        }
    }
}
