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
                sum.AddToSelf(op);

            count += 1;
        }

        public ExpressionOperand FinalValue()
        {
            if (sum.NodeType == ExpressionOperandType.DECIMAL)
                return ExpressionOperand.DecimalFromDouble(sum.AsDouble() / count);
            else if (sum.NodeType == ExpressionOperandType.INTEGER)
                return ExpressionOperand.IntegerFromInt(sum.AsInteger() / count);
            else
                throw new InvalidOperationException();
        }
    }
}

