namespace JankSQL.Contexts
{
    using JankSQL.Operators;

    internal class AggregateContext
    {

        internal AggregateContext(AggregationOperatorType aggregationType, Expression expression, int expressionID)
        {
            this.Expression = expression;
            this.AggregationOperatorType = aggregationType;
            ExpressionName = $"EXPR{expressionID}";
        }

        internal AggregateContext(AggregationOperatorType aggregationType, Expression expression, string expressionName)
        {
            this.Expression = expression;
            this.AggregationOperatorType = aggregationType;
            this.ExpressionName = expressionName;
        }

        internal string? ExpressionName { get; set; }

        internal Expression Expression { get; }

        internal AggregationOperatorType AggregationOperatorType { get; }

        public object Clone()
        {
            AggregateContext clone = new(AggregationOperatorType, Expression, ExpressionName);
            return clone;
        }

        internal void Dump()
        {
            Console.WriteLine("=====");
            Console.WriteLine($"{AggregationOperatorType} aggregation on {Expression}");
        }
    }
}
