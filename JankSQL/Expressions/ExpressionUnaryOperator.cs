namespace JankSQL
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


        internal override ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            if (Str == "~")
            {
                ExpressionOperand op = stack.Pop();
                ExpressionOperand result = op.OperatorUnaryTilde();
                return result;
            }
            else if (Str == "+")
            {
                ExpressionOperand op = stack.Pop();
                ExpressionOperand result = op.OperatorUnaryPlus();
                return result;
            }
            else if (Str == "-")
            {
                ExpressionOperand op = stack.Pop();
                ExpressionOperand result = op.OperatorUnaryMinus();
                return result;
            }
            else
            {
                throw new NotImplementedException($"ExpressionOperator: no implementation for {Str}");
            }
        }
    }
}
