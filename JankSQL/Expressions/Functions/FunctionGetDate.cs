namespace JankSQL.Expressions.Functions
{
    internal class FunctionGetDate : ExpressionFunction
    {
        internal FunctionGetDate()
            : base("GetDate")
        {
        }

        internal override int ExpectedParameters => 0;

        internal override ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            return ExpressionOperand.DateTimeFromDateTime(DateTime.UtcNow);
        }
    }
}

