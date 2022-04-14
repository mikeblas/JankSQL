namespace JankSQL.Expressions
{
    internal class ExpressionInOperator : ExpressionNode
    {
        private readonly List<Expression> targets;
        private readonly bool notIn;

        internal ExpressionInOperator(bool notIn, List<Expression> targets)
        {
            this.targets = targets;
            this.notIn = notIn;
        }

        public override string ToString()
        {
            return "IN Operator";
        }

        internal ExpressionOperand Evaluate(Engines.IEngine engine, IRowValueAccessor accessor, Stack<ExpressionOperand> stack)
        {
            bool result = false;

            ExpressionOperand left = stack.Pop();

            // see if we find one that matches
            for (int i = 0; i < targets.Count; i++)
            {
                ExpressionOperand target = targets[i].Evaluate(accessor, engine);
                if (left.OperatorEquals(target))
                {
                    result = true;
                    break;
                }
            }

            // invert?
            if (notIn)
                result = !result;

            // return what we discovered
            ExpressionOperand r = new ExpressionOperandBoolean(result);
            return r;
        }
    }
}

