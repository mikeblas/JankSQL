namespace JankSQL.Expressions
{
    /// <summary>
    /// wraps a discrete row and a list of column names to be used by
    /// the Expression.Evaluate() method.
    /// </summary>
    internal class TemporaryRowValueAccessor : IRowValueAccessor
    {
        private readonly FullColumnName[] names;
        private readonly Tuple rowData;

        internal TemporaryRowValueAccessor(Tuple rowData, IEnumerable<FullColumnName> names)
        {
            this.names = names.ToArray();
            this.rowData = rowData;
        }

        ExpressionOperand IRowValueAccessor.GetValue(FullColumnName fcn)
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Equals(fcn))
                    return rowData[i];
            }

            throw new ExecutionException($"column {fcn} not found in TemporaryRowValueAccessor");
        }

        void IRowValueAccessor.SetValue(FullColumnName fcn, ExpressionOperand op)
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Equals(fcn))
                {
                    rowData[i] = op;
                    return;
                }
            }

            throw new ExecutionException($"column {fcn} not found in TemporaryRowValueAccessor");
        }

    }
}
