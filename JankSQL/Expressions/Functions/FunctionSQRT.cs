namespace JankSQL.Expressions.Functions
{
    internal class FunctionSQRT : ExpressionFunction
    {
        internal FunctionSQRT()
            : base("SQRT")
        {
        }

        internal override int ExpectedParameters => 1;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            ExpressionOperand op1 = stack.Pop();
            ExpressionOperand result;

            if (op1.RepresentsNull)
                result = ExpressionOperand.NullLiteral();
            else
            {
                double d = Math.Sqrt(op1.AsDouble());
                result = ExpressionOperand.DecimalFromDouble(d);
            }

            stack.Push(result);
        }
    }
}

