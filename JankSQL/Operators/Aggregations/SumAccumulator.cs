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
            {
                if (!sum.RepresentsNull)
                {
                    if (op.RepresentsNull)
                        sum = ExpressionOperand.NullLiteral();
                    else
                        sum.AddToSelf(op);
                }
            }
        }

        public ExpressionOperand FinalValue()
        {
            // a sum of no rows is NULL
            if (sum == null)
                return ExpressionOperand.NullLiteral();

            // otherwise, we return our sum
            return sum;
        }
    }
}

