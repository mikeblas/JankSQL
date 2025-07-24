
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

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, IDictionary<string, ExpressionOperand> bindValues)
        {
            if (!bindValues.TryGetValue(targetName, out bindValue))
                throw new SemanticErrorException($"Bind target {targetName} was not bound");

            stack.Push(bindValue);
        }

        internal override BindResult Bind(Engines.IEngine engine, IList<FullColumnName> columns, IList<FullColumnName> outerColumns, IDictionary<string, ExpressionOperand> bindValues)
        {
            if (!bindValues.ContainsKey(targetName))
                return BindResult.Failed($"Bind target {targetName} was not bound");

            return BindResult.Success();
        }
    }
}
