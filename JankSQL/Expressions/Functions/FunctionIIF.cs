namespace JankSQL.Expressions.Functions
{
    internal class FunctionIIF : ExpressionFunction
    {
        internal FunctionIIF()
            : base("IIF")
        {
        }

        internal override int ExpectedParameters => 2;

        internal override ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            ExpressionOperand right = stack.Pop();
            ExpressionOperand left = stack.Pop();
            ExpressionOperand condition = stack.Pop();

            if (condition.IsTrue())
                return left;
            else
                return right;
        }
    }
}
