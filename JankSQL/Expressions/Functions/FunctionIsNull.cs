namespace JankSQL.Expressions.Functions
{
    using Antlr4.Runtime;

    internal class FunctionIsNull : ExpressionFunction
    {
        internal FunctionIsNull()
            : base("ISNULL")
        {
        }

        internal override int ExpectedParameters => 2;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, IDictionary<string, ExpressionOperand> bindValues)
        {
            ExpressionOperand right = stack.Pop();
            ExpressionOperand left = stack.Pop();

            ExpressionOperand result = left.RepresentsNull ? right : left;
            stack.Push(result);
        }

        internal override void SetFromBuiltInFunctionsContext(IList<ParserRuleContext> stack, TSqlParser.Built_in_functionsContext bifContext)
        {
            var c = (TSqlParser.ISNULLContext)bifContext;
            stack.Add(c.left);
            stack.Add(c.right);
        }
    }
}
