
namespace JankSQL.Expressions.Functions
{
    internal class FunctionIsNull : ExpressionFunction
    {
        internal FunctionIsNull()
            : base("ISNULL")
        {
        }

        internal override int ExpectedParameters => 2;

        internal override ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            ExpressionOperand right = stack.Pop();
            ExpressionOperand left = stack.Pop();

            if (left.RepresentsNull)
                return right;
            return left;
        }
    }
}
