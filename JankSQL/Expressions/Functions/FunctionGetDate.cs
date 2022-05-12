namespace JankSQL.Expressions.Functions
{
    internal class FunctionGetDate : ExpressionFunction
    {
        internal FunctionGetDate()
            : base("GetDate")
        {
        }

        internal override int ExpectedParameters => 0;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            EvaluateContained(stack);
        }
        internal override void EvaluateContained(Stack<ExpressionOperand> stack)
        {
            stack.Push(ExpressionOperand.DateTimeFromDateTime(DateTime.UtcNow));
        }
    }
}

