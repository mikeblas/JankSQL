namespace JankSQL
{
    /// <summary>
    /// Wraps a Rowset and accepts a row index so an individual row
    /// can be accessed by Expression.Evaluate().
    /// </summary>
    internal class ResultSetValueAccessor : IRowValueAccessor
    {
        //TODO: refactor this into Operators namespace
        private readonly ResultSet resultSet;
        private readonly int rowIndex;

        internal ResultSetValueAccessor(ResultSet resultSet, int rowIndex)
        {
            this.resultSet = resultSet;
            this.rowIndex = rowIndex;
        }

        public ExpressionOperand GetValue(FullColumnName fcn)
        {
            int idx = resultSet.ColumnIndex(fcn);
            if (idx == -1)
                throw new ExecutionException($"Invalid column name {fcn}; valid names are {string.Join(",", resultSet.GetColumnNames())}");

            ExpressionOperand[] thisRow = resultSet.Row(rowIndex);
            ExpressionOperand val = thisRow[idx];
            return val;
        }

        public void SetValue(FullColumnName fcn, ExpressionOperand op)
        {
            throw new NotImplementedException();
        }
    }
}
