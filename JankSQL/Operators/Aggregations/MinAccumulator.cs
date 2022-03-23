namespace JankSQL.Operators.Aggregations
{
    internal class MinAccumulator : IAggregateAccumulator
    {
        private ExpressionOperand? min;

        internal MinAccumulator()
        {
        }

        public void Accumulate(ExpressionOperand op)
        {
            if (min == null)
                min = op;
            else
            {
                int comp = min.CompareTo(op);
                if (comp > 0)
                    min = op;
            }
        }

        public ExpressionOperand FinalValue()
        {
            return min;
        }
    }
}

