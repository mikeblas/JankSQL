namespace JankSQL.Expressions.Functions
{
    internal class FunctionSQRT : ExpressionFunction
    {
        internal FunctionSQRT()
            : base("SQRT")
        {
        }

        internal override int ExpectedParameters => 1;

        internal override ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            ExpressionOperand op1 = stack.Pop();
            double d = Math.Sqrt(op1.AsDouble());
            ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
            return result;
        }
    }
}

