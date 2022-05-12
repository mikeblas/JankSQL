namespace JankSQL.Expressions.Functions
{
    internal class FunctionIIF : ExpressionFunction
    {
        internal FunctionIIF()
            : base("IIF")
        {
        }

        internal override int ExpectedParameters => 2;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            EvaluateContained(stack);
        }

        internal override void EvaluateContained(Stack<ExpressionOperand> stack)
        {
            ExpressionOperand right = stack.Pop();
            ExpressionOperand left = stack.Pop();
            ExpressionOperand condition = stack.Pop();

            ExpressionOperand result = condition.IsTrue() ? left : right;
            stack.Push(result);
        }
    }
}
