namespace JankSQL.Expressions.Functions
{
    using Antlr4.Runtime;

    internal class FunctionLEN : ExpressionFunction
    {
        internal FunctionLEN()
            : base("LEN")
        {
        }

        internal override int ExpectedParameters => 1;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            EvaluateContained(stack);
        }

        internal override void EvaluateContained(Stack<ExpressionOperand> stack)
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

        internal override void SetFromBuiltInFunctionsContext(IList<ParserRuleContext> stack, TSqlParser.Built_in_functionsContext bifContext)
        {
            var c = (TSqlParser.LENContext)bifContext;
            stack.Add(c.string_expression);
        }
    }
}

