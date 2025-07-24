namespace JankSQL.Expressions.Functions
{
    using Antlr4.Runtime;

    internal class FunctionPI : ExpressionFunction
    {
        internal FunctionPI()
            : base("PI")
        {
        }

        internal override int ExpectedParameters => 0;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, IDictionary<string, ExpressionOperand> bindValues)
        {
            stack.Push(ExpressionOperand.DecimalFromDouble(Math.PI));
        }

        internal override void SetFromBuiltInFunctionsContext(IList<ParserRuleContext> stack, TSqlParser.Built_in_functionsContext bifContext)
        {
            // no params, nothing to do
        }
    }
}

