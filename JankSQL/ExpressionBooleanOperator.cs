namespace JankSQL
{
    internal class ExpressionBooleanOperator : ExpressionNode
    {
        enum BooleanOperatorType
        {
            AND, OR, NOT
        }

        BooleanOperatorType opType;

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

        ExpressionBooleanOperator(BooleanOperatorType opType)
        {
            this.opType = opType;
        }

        public override String ToString()
        {
            return opType.ToString();
        }

        internal ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            bool result = true;

            ExpressionOperand right = (ExpressionOperand)stack.Pop();

            switch (opType)
            {
                case BooleanOperatorType.AND:
                    {
                        ExpressionOperand left = (ExpressionOperand)stack.Pop();
                        result = right.IsTrue() && left.IsTrue();
                    }
                    break;

                case BooleanOperatorType.OR:
                    {
                        ExpressionOperand left = (ExpressionOperand)stack.Pop();
                        result = right.IsTrue() || left.IsTrue();
                    }
                    break;

                case BooleanOperatorType.NOT:
                    result = !right.IsTrue();
                    break;
            }

            return new ExpressionOperandBoolean(result);
        }
    }
}

