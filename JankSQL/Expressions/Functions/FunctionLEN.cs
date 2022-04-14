namespace JankSQL.Expressions.Functions
{
    internal class FunctionLEN : ExpressionFunction
    {
        internal FunctionLEN()
            : base("LEN")
        {
        }

        internal override int ExpectedParameters => 1;

        internal override ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            ExpressionOperand op1 = stack.Pop();
            if (op1.RepresentsNull)
                return ExpressionOperand.NullLiteral();

            int resultLen = op1.AsString().Length;
            ExpressionOperand result = ExpressionOperand.IntegerFromInt(resultLen);
            return result;
        }
    }
}

