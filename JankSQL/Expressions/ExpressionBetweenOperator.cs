namespace JankSQL.Expressions
{
    internal class ExpressionBetweenOperator : ExpressionNode
    {
        private readonly bool isNotBetween;

        internal ExpressionBetweenOperator(bool notBetween)
        {
            isNotBetween = notBetween;
        }

        public override string ToString()
        {
            return isNotBetween ? "NOT BETWEEN" : "BETWEEN";
        }

        internal ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            bool result;
            ExpressionOperand right = stack.Pop();
            ExpressionOperand left = stack.Pop();
            ExpressionOperand value = stack.Pop();

            result = value.OperatorLessThan(left) || value.OperatorGreaterThan(right);

            if (!isNotBetween)
                result = !result;

            return new ExpressionOperandBoolean(result);
        }
    }
}




