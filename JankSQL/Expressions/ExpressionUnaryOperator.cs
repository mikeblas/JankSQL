namespace JankSQL.Expressions
{
    public class ExpressionUnaryOperator : ExpressionOperator
    {
        internal ExpressionUnaryOperator(string str)
            : base(str)
        {
        }

        internal static ExpressionUnaryOperator GetUnaryOperator(TSqlParser.Unary_operator_expressionContext context)
        {
            if (context.BIT_NOT() != null)
                return new ExpressionUnaryOperator("~");
            else
                return new ExpressionUnaryOperator(context.op.Text);
        }


        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, IDictionary<string, ExpressionOperand> bindValues)
        {
            if (Str == "~")
            {
                ExpressionOperand op = stack.Pop();
                ExpressionOperand result = op.OperatorUnaryTilde();
                stack.Push(result);
            }
            else if (Str == "+")
            {
                ExpressionOperand op = stack.Pop();
                ExpressionOperand result = op.OperatorUnaryPlus();
                stack.Push(result);
            }
            else if (Str == "-")
            {
                ExpressionOperand op = stack.Pop();
                ExpressionOperand result = op.OperatorUnaryMinus();
                stack.Push(result);
            }
            else
            {
                throw new NotImplementedException($"ExpressionOperator: no implementation for {Str}");
            }
        }
    }
}
