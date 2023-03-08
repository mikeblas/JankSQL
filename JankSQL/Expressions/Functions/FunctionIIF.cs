namespace JankSQL.Expressions.Functions
{
    using Antlr4.Runtime;

    internal class FunctionIIF : ExpressionFunction
    {
        internal FunctionIIF()
            : base("IIF")
        {
        }

        internal override int ExpectedParameters => 2;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            ExpressionOperand right = stack.Pop();
            ExpressionOperand left = stack.Pop();
            ExpressionOperand condition = stack.Pop();

            ExpressionOperand result = condition.IsTrue() ? left : right;
            stack.Push(result);
        }

        internal override void SetFromBuiltInFunctionsContext(IList<ParserRuleContext> stack, TSqlParser.Built_in_functionsContext bifContext)
        {
            var c = (TSqlParser.IIFContext)bifContext;
            stack.Add(c.cond);
            stack.Add(c.left);
            stack.Add(c.right);
        }
    }
}
