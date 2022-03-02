﻿namespace JankSQL
{
    internal class ExpressionComparisonOperator : ExpressionNode
    {
        internal string str;
        internal ExpressionComparisonOperator(string str)
        {
            this.str = str;
        }

        public override String ToString()
        {
            return str;
        }

        internal ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            bool result;

            if (str == ">")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                result = left.OperatorGreaterThan(right);
            }
            else if (str == "<")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                result = left.OperatorLessThan(right);
            }
            else if (str == "=")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                result = left.OperatorEquals(right);
            }
            else if (str == "<>" || str == "!=")
            {
                ExpressionOperand right = stack.Pop();
                ExpressionOperand left = stack.Pop();

                result = !left.OperatorEquals(right);
            }
            else
            {
                throw new NotImplementedException($"ExpressionOperator: no implementation for {str}");
            }

            return new ExpressionOperandBoolean(result);
        }
    }
}
