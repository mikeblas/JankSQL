namespace JankSQL.Expressions
{
    internal class CombinedValueAccessor : IRowValueAccessor
    {
        private readonly IRowValueAccessor inner;
        private readonly IRowValueAccessor? outer;

        internal CombinedValueAccessor(IRowValueAccessor inner, IRowValueAccessor? outer)
        {
            this.inner = inner;
            this.outer = outer;
        }

        public ExpressionOperand GetValue(FullColumnName fcn)
        {
            if (outer != null)
            {
                try
                {
                    ExpressionOperand val = outer.GetValue(fcn);
                    return val;
                }
                catch (ExecutionException)
                {
                }
            }

            return inner.GetValue(fcn);
        }

        public void SetValue(FullColumnName fcn, ExpressionOperand op)
        {
            throw new NotImplementedException();
        }
    }
}
