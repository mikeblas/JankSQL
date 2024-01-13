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

        public ExpressionOperand GetValue(FullColumnName fullColumnName)
        {
            if (outer != null)
            {
                try
                {
                    ExpressionOperand val = outer.GetValue(fullColumnName);
                    return val;
                }
                catch (ExecutionException)
                {
                }
            }

            return inner.GetValue(fullColumnName);
        }

        public void SetValue(FullColumnName fullColumnName, ExpressionOperand op)
        {
            throw new NotImplementedException();
        }
    }
}
