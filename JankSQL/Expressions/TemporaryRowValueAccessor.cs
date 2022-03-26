namespace JankSQL
{
    /// <summary>
    /// wraps a discrete row and a list of column names to be used by
    /// the Expression.Evaluate() method.
    /// </summary>
    internal class TemporaryRowValueAccessor : IRowValueAccessor
    {
        private readonly ColumnNameList names;
        private readonly Tuple rowData;

        internal TemporaryRowValueAccessor(Tuple rowData, ColumnNameList names)
        {
            this.names = names;
            this.rowData = rowData;
        }

        ExpressionOperand IRowValueAccessor.GetValue(FullColumnName fcn)
        {
            int index = names.GetColumnIndex(fcn);
            if (index == -1)
                throw new ExecutionException($"column {fcn} not found in TemporaryRowValueAccessor");

            return rowData[index];
        }

        void IRowValueAccessor.SetValue(FullColumnName fcn, ExpressionOperand op)
        {
            int index = names.GetColumnIndex(fcn);
            if (index == -1)
                throw new ExecutionException($"column {fcn} not found in TemporaryRowValueAccessor");

            rowData[index] = op;
        }
    }
}
