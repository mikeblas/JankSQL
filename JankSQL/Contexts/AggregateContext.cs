namespace JankSQL.Contexts
{
    using JankSQL.Operators;

    internal class AggregateContext
    {
        private readonly AggregationOperatorType aggregationOperatorType;
        private readonly Expression expression;
        private string? expressionName;

        internal AggregateContext(AggregationOperatorType aggregationType, Expression expression, int expressionID)
        {
            this.expression = expression;
            this.aggregationOperatorType = aggregationType;
            expressionName = $"EXPR{expressionID}";
        }

        internal AggregateContext(AggregationOperatorType aggregationType, Expression expression)
        {
            this.expression = expression;
            this.aggregationOperatorType = aggregationType;
        }

        internal string? ExpressionName
        {
            get { return expressionName; } set { expressionName = value; }
        }

        internal Expression Expression
        {
            get { return expression; }
        }

        internal AggregationOperatorType AggregationOperatorType
        {
            get { return aggregationOperatorType; }
        }

        public object Clone()
        {
            AggregateContext clone = new AggregateContext(aggregationOperatorType, expression);
            clone.expressionName = ExpressionName;
            return clone;
        }

        internal void Dump()
        {
            Console.WriteLine("=====");
            Console.WriteLine($"{aggregationOperatorType} aggregation on {expression}");
        }
    }
}
