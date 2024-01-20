namespace JankSQL.Expressions
{
    /// <summary>
    /// Wraps a ResultSet and accepts a row index so an individual row
    /// can be accessed by Expression.Evaluate().
    /// </summary>
    internal class ResultSetValueAccessor : IRowValueAccessor
    {
        private readonly ResultSet resultSet;
        private readonly int rowIndex;

        internal ResultSetValueAccessor(ResultSet resultSet, int rowIndex)
        {
            this.resultSet = resultSet;
            this.rowIndex = rowIndex;
        }

        public ExpressionOperand GetValue(FullColumnName fullColumnName)
        {
            int idx = resultSet.ColumnIndex(fullColumnName);
            if (idx == -1)
                throw new ExecutionException($"Invalid column name {fullColumnName}; valid names are {string.Join(", ", resultSet.GetColumnNames())}");

            Tuple thisRow = resultSet.Row(rowIndex);
            ExpressionOperand val = thisRow[idx];
            return val;
        }

        public bool TryGetValue(FullColumnName fullColumnName, out ExpressionOperand? value)
        {
            int idx = resultSet.ColumnIndex(fullColumnName);
            if (idx == -1)
            {
                value = null;
                return false;
            }

            Tuple thisRow = resultSet.Row(rowIndex);
            value = thisRow[idx];
            return true;
        }

        public void SetValue(FullColumnName fullColumnName, ExpressionOperand op)
        {
            throw new NotImplementedException();
        }
    }
}
