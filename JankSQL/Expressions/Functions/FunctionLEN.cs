namespace JankSQL.Expressions.Functions
{
    internal class FunctionLEN : ExpressionFunction
    {
        internal FunctionLEN()
            : base("LEN")
        {
        }

        internal override int ExpectedParameters => 1;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            ExpressionOperand result;
            ExpressionOperand op1 = stack.Pop();

            if (op1.RepresentsNull)
                result = ExpressionOperand.NullLiteral();
            else
            {
                int resultLen = op1.AsString().Length;
                result = ExpressionOperand.IntegerFromInt(resultLen);
            }

            stack.Push(result);
        }
    }
}

