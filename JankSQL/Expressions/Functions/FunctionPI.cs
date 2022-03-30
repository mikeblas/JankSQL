namespace JankSQL.Expressions.Functions
{
    internal class FunctionPI : ExpressionFunction
    {
        internal FunctionPI()
            : base("PI")
        {
        }

        internal override int ExpectedParameters => 0;

        internal override ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            return ExpressionOperand.DecimalFromDouble(Math.PI);
        }
    }
}

