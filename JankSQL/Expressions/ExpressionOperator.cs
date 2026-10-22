namespace JankSQL.Expressions
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
                throw new ArgumentNullException(nameof(other));

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

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, IDictionary<string, ExpressionOperand> bindValues)
        {
            ExpressionOperand result;
            if (str == "/")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                result = left.OperatorSlash(right);
            }
            else if (str == "+")
            {
                ExpressionOperand op1 = stack.Pop();
                ExpressionOperand op2 = stack.Pop();

                result = op2.OperatorPlus(op1);
            }
            else if (str == "-")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                result = left.OperatorMinus(right);
            }
            else if (str == "*")
            {
                ExpressionOperand op1 = stack.Pop();
                ExpressionOperand op2 = stack.Pop();

                result = op1.OperatorTimes(op2);
            }
            else if (str == "%")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                result = left.OperatorModulo(right);
            }
            else
            {
                throw new NotImplementedException($"ExpressionOperator: no implementation for {str}");
            }

            stack.Push(result);
        }
    }
}
