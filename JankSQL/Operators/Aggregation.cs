
namespace JankSQL
{
    internal enum AggregationOperatorType
    {
        AVG, MAX, MIN, SUM, STDEV, STDEVP, VAR, VARP, COUNT, COUNT_BIG
    }

    internal class Aggregation : IComponentOutput
    {
        AggregationOperatorType operatorType;
        Expression expression;

        internal Aggregation(AggregationOperatorType operatorType, Expression expression)
        {
            this.operatorType = operatorType;
            this.expression = expression;
        }

        public ResultSet? GetRows(int max)
        {
            throw new NotImplementedException();
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}
