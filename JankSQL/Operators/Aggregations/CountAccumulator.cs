namespace JankSQL.Operators.Aggregations
{
    internal class CountAccumulator : IAggregateAccumulator
    {
        private int count = 0;

        internal CountAccumulator()
        {
        }

        public void Accumulate(ExpressionOperand op)
        {
            if (!op.RepresentsNull)
                count += 1;
        }

        public ExpressionOperand FinalValue()
        {
            return ExpressionOperand.IntegerFromInt(count);
        }
    }
}

