namespace JankSQL
{
    public class ExpressionOperator : ExpressionNode
    {
        internal string str;

        internal ExpressionOperator(string str)
        {
            this.str = str;
        }

        public override String ToString()
        {
            return str;
        }

        internal ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
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
            else if (str == "SQRT")
            {
                ExpressionOperand op1 = stack.Pop();
                double d = Math.Sqrt(op1.AsDouble());
                ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
                return result;
            }
            else if (str == "POWER")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                double d = Math.Pow(left.AsDouble(), right.AsDouble());
                ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
                return result;
            }
            else if (str == "PI")
            {
                double d = Math.PI;
                ExpressionOperand result = ExpressionOperand.DecimalFromDouble(d);
                return result;
            }
            else
            {
                throw new NotImplementedException($"ExpressionOperator: no implementation for {str}");
            }
        }
    }
}

