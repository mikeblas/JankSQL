namespace JankSQL.Expressions
{
    internal class ExpressionBindOperator : ExpressionNode
    {
        private readonly string targetName;
        private ExpressionOperand? bindValue;

        internal ExpressionBindOperator(string targetName)
        {
            this.targetName = targetName;
        }

        public override string ToString()
        {
            return $"BindValue({targetName}";
        }

        internal ExpressionOperand Evaluate(Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            if (bindValue == null)
            {
                if (!bindValues.TryGetValue(targetName, out bindValue))
                    throw new SemanticErrorException($"Bind target {targetName} was not bound");
            }

            return bindValue;
        }
    }
}

