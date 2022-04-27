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
            stack.Push(ExpressionOperand.DateTimeFromDateTime(DateTime.UtcNow));
        }
    }
}

