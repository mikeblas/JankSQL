namespace JankSQL.Expressions.Functions
{
    internal class FunctionIsNull : ExpressionFunction
    {
        internal FunctionIsNull()
            : base("ISNULL")
        {
        }

        internal override int ExpectedParameters => 2;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            ExpressionOperand right = stack.Pop();
            ExpressionOperand left = stack.Pop();

            ExpressionOperand result = left.RepresentsNull ? right : left;
            stack.Push(result);
        }
    }
}
