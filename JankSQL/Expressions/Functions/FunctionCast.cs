namespace JankSQL.Expressions.Functions
{
    using Antlr4.Runtime;
    using static TSqlParser;

    internal class FunctionCast : ExpressionFunction
    {
        private ExpressionOperandType targetType;

        internal FunctionCast()
            : base("CAST")
        {
        }

        internal override int ExpectedParameters => 2;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            EvaluateContained(stack);
        }

        internal override void EvaluateContained(Stack<ExpressionOperand> stack)
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

        internal override void SetFromBuiltInFunctionsContext(IList<ParserRuleContext> stack, TSqlParser.Built_in_functionsContext bifContext)
        {
            var c = (TSqlParser.CASTContext)bifContext;

            targetType = JankListener.GobbleDataType(c.data_type());
            stack.Add(c.expression());
        }
    }
}
