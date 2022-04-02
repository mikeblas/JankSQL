namespace JankSQL
{
    public class ExpressionOperator : ExpressionNode, IEquatable<ExpressionOperator>
    {
        private readonly string str;

        internal ExpressionOperator(string str)
        {
            this.str = str;
        }

        internal string Str
        {
            get { return str; }
        }

        public bool Equals(ExpressionOperator? other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (ReferenceEquals(this, other))
                return true;

            return str.Equals(other.str, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ExpressionOperator);
        }

        public override int GetHashCode()
        {
            return str.GetHashCode();
        }

        public override string ToString()
        {
            return str;
        }

        internal virtual ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            if (str == "/")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                ExpressionOperand result = left.OperatorSlash(right);
                return result;
            }
            else if (str == "+")
            {
                ExpressionOperand op1 = stack.Pop();
                ExpressionOperand op2 = stack.Pop();

                ExpressionOperand result = op2.OperatorPlus(op1);
                return result;
            }
            else if (str == "-")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                ExpressionOperand result = left.OperatorMinus(right);
                return result;
            }
            else if (str == "*")
            {
                ExpressionOperand op1 = stack.Pop();
                ExpressionOperand op2 = stack.Pop();

                ExpressionOperand result = op1.OperatorTimes(op2);
                return result;
            }
            else if (str == "%")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                ExpressionOperand result = left.OperatorModulo(right);
                return result;
            }
            else
            {
                throw new NotImplementedException($"ExpressionOperator: no implementation for {str}");
            }
        }
    }
}

