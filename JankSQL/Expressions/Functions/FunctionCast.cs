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
                result = targetType switch
                {
                    ExpressionOperandType.INTEGER => ExpressionOperand.IntegerFromInt(op.AsInteger()),
                    ExpressionOperandType.VARCHAR => ExpressionOperand.VARCHARFromString(op.AsString()),
                    ExpressionOperandType.DECIMAL => ExpressionOperand.DecimalFromDouble(op.AsDouble()),
                    ExpressionOperandType.DATETIME => ExpressionOperand.DateTimeFromDateTime(op.AsDateTime()),
                    _ => throw new NotImplementedException($"type {targetType} not supported by CAST"),
                };
            }
            catch (FormatException)
            {
                throw new ExecutionException($"failed to convert {op} to {targetType}");
            }

            stack.Push(result);
        }
    }
}
