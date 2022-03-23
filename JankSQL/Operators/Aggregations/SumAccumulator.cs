namespace JankSQL.Operators.Aggregations
{
    internal class SumAccumulator : IAggregateAccumulator
    {
        private ExpressionOperand? sum;

        internal SumAccumulator()
        {
        }

        public void Accumulate(ExpressionOperand op)
        {
            if (sum == null)
                sum = op;
            else
                sum.AddToSelf(op);
        }

        public ExpressionOperand FinalValue()
        {
            return sum;
        }
    }
}

