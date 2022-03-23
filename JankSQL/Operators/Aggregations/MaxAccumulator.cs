namespace JankSQL.Operators.Aggregations
{
    internal class MaxAccumulator : IAggregateAccumulator
    {
        private ExpressionOperand? max;

        internal MaxAccumulator()
        {
        }

        public void Accumulate(ExpressionOperand op)
        {
            if (max == null)
                max = op;
            else
            {
                int comp = max.CompareTo(op);
                if (comp < 0)
                    max = op;
            }
        }

        public ExpressionOperand FinalValue()
        {
            return max;
        }
    }
}

