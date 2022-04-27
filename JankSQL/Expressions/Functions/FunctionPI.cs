namespace JankSQL.Expressions.Functions
{
    internal class FunctionPI : ExpressionFunction
    {
        internal FunctionPI()
            : base("PI")
        {
        }

        internal override int ExpectedParameters => 0;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            stack.Push(ExpressionOperand.DecimalFromDouble(Math.PI));
        }
    }
}

