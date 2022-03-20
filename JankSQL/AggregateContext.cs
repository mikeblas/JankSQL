
namespace JankSQL
{
    internal enum AggregationOperatorType
    {
        AVG, MAX, MIN, SUM, STDEV, STDEVP, VAR, VARP, COUNT, COUNT_BIG
    }


    internal class AggregateContext
    {
        private AggregationOperatorType aggregationOperatorType;
        readonly Expression expression;
        string? expressionName;

        internal string? ExpressionName { get { return expressionName; } set { expressionName = value; } }

        internal Expression Expression { get { return expression; } }

        internal AggregationOperatorType AggregationOperatorType { get { return aggregationOperatorType; } }

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

    }
}
