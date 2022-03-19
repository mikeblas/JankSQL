
namespace JankSQL
{
    /// <summary>
    /// Wraps a Rowset and accepts a row index so an individual row
    /// can be accessed by Expression.Evaluate()
    /// </summary>
    internal class RowsetValueAccessor : IRowValueAccessor
    {
        ResultSet resultSet;
        int rowIndex;

        internal RowsetValueAccessor(ResultSet resultSet, int rowIndex)
        {
            this.resultSet = resultSet;
            this.rowIndex = rowIndex;
        }

        ExpressionOperand IRowValueAccessor.GetValue(FullColumnName fcn)
        {
            int idx = resultSet.ColumnIndex(fcn);
            if (idx == -1)
                throw new ExecutionException($"Invalid column name {fcn}; valid names are {String.Join(",", resultSet.GetColumnNames())}");

            ExpressionOperand[] thisRow = resultSet.Row(rowIndex);
            ExpressionOperand val = thisRow[idx];
            return val;
        }

        void IRowValueAccessor.SetValue(FullColumnName fcn, ExpressionOperand op)
        {
            throw new NotImplementedException();
        }
    }
}
