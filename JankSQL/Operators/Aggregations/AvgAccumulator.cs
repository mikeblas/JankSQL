namespace JankSQL.Operators.Aggregations
{
    internal class AvgAccumulator : IAggregateAccumulator
    {
        private int count = 0;
        private ExpressionOperand? sum;

        internal AvgAccumulator()
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

            count += 1;
        }

        public ExpressionOperand FinalValue()
        {
            // if we have no rows, then our result is NULL
            if (sum == null || sum.RepresentsNull)
                return ExpressionOperand.NullLiteral();

            if (sum.NodeType == ExpressionOperandType.DECIMAL)
                return ExpressionOperand.DecimalFromDouble(sum.AsDouble() / count);
            else if (sum.NodeType == ExpressionOperandType.INTEGER)
                return ExpressionOperand.IntegerFromInt(sum.AsInteger() / count);
            else
                throw new InvalidOperationException();
        }
    }
}