namespace JankSQL
{
    internal class ExpressionBooleanOperator : ExpressionNode
    {
        private readonly BooleanOperatorType opType;

        internal enum BooleanOperatorType
        {
            AND,
            OR,
            NOT,
        }

        internal ExpressionBooleanOperator(BooleanOperatorType opType)
        {
            this.opType = opType;
        }

        internal static ExpressionBooleanOperator GetOrOperator()
        {
            return new ExpressionBooleanOperator(BooleanOperatorType.OR);
        }

        internal static ExpressionBooleanOperator GetAndOperator()
        {
            return new ExpressionBooleanOperator(BooleanOperatorType.AND);
        }

        internal static ExpressionBooleanOperator GetNotOperator()
        {
            return new ExpressionBooleanOperator(BooleanOperatorType.NOT);
        }

        public override string ToString()
        {
            return opType.ToString();
        }

        internal ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            bool result = true;

            ExpressionOperand right = stack.Pop();

            switch (opType)
            {
                case BooleanOperatorType.AND:
                    ExpressionOperand leftAnd = stack.Pop();
                    result = right.IsTrue() && leftAnd.IsTrue();
                    break;

                case BooleanOperatorType.OR:
                    ExpressionOperand leftOr = stack.Pop();
                    result = right.IsTrue() || leftOr.IsTrue();
                    break;

                case BooleanOperatorType.NOT:
                    result = !right.IsTrue();
                    break;
            }

            return new ExpressionOperandBoolean(result);
        }
    }
}

