
namespace JankSQL
{
    internal enum AggregationOperatorType
    {
        AVG, MAX, MIN, SUM, STDEV, STDEVP, VAR, VARP, COUNT, COUNT_BIG
    }


    internal class AggregateContext
    {
        private AggregationOperatorType aggregationOperatorType;
        Expression expression;

        internal AggregateContext(AggregationOperatorType aggregationType, Expression expression)
        {
            this.expression = expression;
            this.aggregationOperatorType = aggregationType;
        }
    }
}
