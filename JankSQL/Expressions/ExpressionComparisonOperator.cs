namespace JankSQL.Expressions
{
    internal class ExpressionComparisonOperator : ExpressionNode
    {
        private readonly string str;

        internal ExpressionComparisonOperator(string str)
        {
            this.str = str;
        }

        public override string ToString()
        {
            return str;
        }

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            bool result;
            ExpressionOperand right = stack.Pop();
            ExpressionOperand left = stack.Pop();

            if (str == ">")
                result = left.OperatorGreaterThan(right);
            else if (str == "<")
                result = left.OperatorLessThan(right);
            else if (str == "=")
                result = left.OperatorEquals(right);
            else if (str == "<>" || str == "!=")
                result = !left.OperatorEquals(right);
            else if (str == ">=" || str == "!<")
                result = left.OperatorGreaterThan(right) || left.OperatorEquals(right);
            else if (str == "<=" || str == "!>")
                result = left.OperatorLessThan(right) || left.OperatorEquals(right);
            else
                throw new NotImplementedException($"ExpressionComparisonOperator: no implementation for {str}");

            stack.Push(new ExpressionOperandBoolean(result));
        }
    }
}

