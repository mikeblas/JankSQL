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
                ExpressionOperand? value;
                if (outer.TryGetValue(fullColumnName, out value))
                    return value!;
            }

            return inner.GetValue(fullColumnName);
        }

        public bool TryGetValue(FullColumnName fullColumnName, out ExpressionOperand? value)
        {
            if (outer != null)
            {
                if (outer.TryGetValue(fullColumnName, out value))
                    return true;
            }

            return inner.TryGetValue(fullColumnName, out value);
        }

        public void SetValue(FullColumnName fullColumnName, ExpressionOperand op)
        {
            throw new NotImplementedException();
        }
    }
}
