namespace JankSQL.Expressions.Functions
{
    using Antlr4.Runtime;

    internal class FunctionGetDate : ExpressionFunction
    {
        internal FunctionGetDate()
            : base("GetDate")
        {
        }

        internal override int ExpectedParameters => 0;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, IDictionary<string, ExpressionOperand> bindValues)
        {
            stack.Push(ExpressionOperand.DateTimeFromDateTime(DateTime.UtcNow));
        }

        internal override void SetFromBuiltInFunctionsContext(IList<ParserRuleContext> stack, TSqlParser.Built_in_functionsContext bifContext)
        {
            // no args, nothing to do
        }
    }
}

