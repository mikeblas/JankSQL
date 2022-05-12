namespace JankSQL.Expressions
{
    public class ExpressionComparisonOperator : ExpressionNode
    {
        private readonly string str;

        internal ExpressionComparisonOperator(string str)
        {
            this.str = str;
        }

        internal bool IsEquality
        {
            get { return str == "="; }
        }

        public override string ToString()
        {
            return str;
        }

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            EvaluateContained(stack);
        }

        internal override void EvaluateContained(Stack<ExpressionOperand> stack)
        {
            ExpressionOperand right = stack.Pop();
            ExpressionOperand left = stack.Pop();

            bool result = DirectEvaluate(left, right);

            stack.Push(new ExpressionOperandBoolean(result));
        }

        internal bool DirectEvaluate(ExpressionOperand left, ExpressionOperand right)
        {
            bool result;

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

            return result;
        }
    }
}

