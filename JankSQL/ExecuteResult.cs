namespace JankSQL
{
    public class ExecuteResult
    {
        private ResultSet? resultSet;

        public ExecuteResult(ExecuteStatus status, string message)
        {
            this.ErrorMessage = message;
            this.ExecuteStatus = status;
        }

        internal ExecuteResult()
        {
            this.ExecuteStatus = ExecuteStatus.NOT_EXECUTED;
        }

        public ResultSet? ResultSet
        {
            get { return resultSet; }
            set { resultSet = value; }
        }

        public ExecuteStatus ExecuteStatus { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
