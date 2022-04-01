namespace JankSQL
{
    public class ExpressionUnaryOperator : ExpressionOperator, IEquatable<ExpressionUnaryOperator>
    {
        internal ExpressionUnaryOperator(string str)
            : base(str)
        {
        }

        public bool Equals(ExpressionUnaryOperator? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (ReferenceEquals(this, other))
                return true;

            return Str.Equals(other.Str, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ExpressionUnaryOperator);
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
