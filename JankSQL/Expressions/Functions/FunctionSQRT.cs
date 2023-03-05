namespace JankSQL.Expressions.Functions
{
    using Antlr4.Runtime;

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

        internal override void SetFromBuiltInFunctionsContext(IList<ParserRuleContext> stack, TSqlParser.Built_in_functionsContext bifContext)
        {
            var c = (TSqlParser.SQRTContext)bifContext;
            stack.Add(c.float_expression);
        }
    }
}

