﻿namespace JankSQL.Expressions
{
    internal class ExpressionIsNullOperator : ExpressionNode
    {
        // true of this is an IS NOT NULL operator
        // false if this is an IS NULL operator
        private readonly bool isNotNull;

        internal ExpressionIsNullOperator(bool isNotNull)
        {
            this.isNotNull = isNotNull;
        }

        public override string ToString()
        {
            return isNotNull ? "IS NOT NULL" : "IS NULL";
        }

        internal ExpressionOperand Evaluate(Stack<ExpressionOperand> stack)
        {
            ExpressionOperand left = stack.Pop();

            bool result = left.RepresentsNull ^ isNotNull;
            return new ExpressionOperandBoolean(result);
        }
    }
}

