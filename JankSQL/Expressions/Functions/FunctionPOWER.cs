namespace JankSQL.Expressions.Functions
{
    internal class FunctionPOWER : ExpressionFunction
    {
        internal FunctionPOWER()
            : base("POWER")
        {
        }

        internal override int ExpectedParameters => 2;

        internal override ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            ExpressionOperand right = stack.Pop();
            ExpressionOperand left = stack.Pop();

            double d = Math.Pow(left.AsDouble(), right.AsDouble());
            ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
            return result;
        }
    }
}

