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

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            ExpressionOperand op = stack.Pop();
            ExpressionOperand result;

            try
            {
                switch (targetType)
                {
                    case ExpressionOperandType.INTEGER:
                        result = ExpressionOperand.IntegerFromInt(op.AsInteger());
                        break;

                    case ExpressionOperandType.VARCHAR:
                        result = ExpressionOperand.VARCHARFromString(op.AsString());
                        break;

                    case ExpressionOperandType.DECIMAL:
                        result = ExpressionOperand.DecimalFromDouble(op.AsDouble());
                        break;

                    case ExpressionOperandType.DATETIME:
                        result = ExpressionOperand.DateTimeFromDateTime(op.AsDateTime());
                        break;

                    default:
                        throw new NotImplementedException($"type {targetType} not supported by CAST");
                }
            }
            catch (FormatException)
            {
                throw new ExecutionException($"failed to convert {op} to {targetType}");
            }

            stack.Push(result);
        }
    }
}
