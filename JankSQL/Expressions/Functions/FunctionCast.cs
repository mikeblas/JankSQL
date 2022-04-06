namespace JankSQL.Expressions.Functions
{
    internal class FunctionCast : ExpressionFunction
    {
        private readonly ExpressionOperandType targetType;

        internal FunctionCast(ExpressionOperandType targetType)
            : base("CAST")
        {
            this.targetType = targetType;
        }

        internal override int ExpectedParameters => 2;

        internal override ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            ExpressionOperand op = stack.Pop();
            ExpressionOperand result;

            switch (targetType)
            {
                case ExpressionOperandType.INTEGER:
                    result = ExpressionOperand.IntegerFromInt(op.AsInteger());
                    break;

                case ExpressionOperandType.VARCHAR:
                    result = ExpressionOperand.VARCHARFromString(op.AsString());
                    break;

                case ExpressionOperandType.NVARCHAR:
                    result = ExpressionOperand.NVARCHARFromString(op.AsString());
                    break;

                case ExpressionOperandType.DECIMAL:
                    result = ExpressionOperand.DecimalFromDouble(op.AsDouble());
                    break;

                default:
                    throw new NotImplementedException($"type {targetType} not supported by CAST");
            }

            return result;
        }
    }
}
