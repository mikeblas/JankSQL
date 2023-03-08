namespace JankSQL
{
    using JankSQL.Contexts;
    using JankSQL.Operators;

    public partial class JankListener : TSqlParserBaseListener
    {
        internal AggregateContext GobbleAggregateFunctionContext(TSqlParser.Aggregate_windowed_functionContext context)
        {
            Expression x = GobbleExpression(context.all_distinct_expression().expression());
            AggregationOperatorType? aot = null;
            if (context.agg_func != null)
            {
                aot = context.agg_func.Type switch
                {
                    TSqlLexer.SUM => AggregationOperatorType.SUM,
                    TSqlLexer.AVG => AggregationOperatorType.AVG,
                    TSqlLexer.MIN => AggregationOperatorType.MIN,
                    TSqlLexer.MAX => AggregationOperatorType.MAX,
                    TSqlLexer.STDEV => AggregationOperatorType.STDEV,
                    TSqlLexer.STDEVP => AggregationOperatorType.STDEVP,
                    TSqlLexer.VAR => AggregationOperatorType.VAR,
                    TSqlLexer.VARP => AggregationOperatorType.VARP,
                    _ => null,
                };
            }
            else if (context.cnt != null)
            {
                aot = context.cnt.Type switch
                {
                    TSqlLexer.COUNT => AggregationOperatorType.COUNT,
                    TSqlLexer.COUNT_BIG => AggregationOperatorType.COUNT_BIG,
                    _ => null,
                };
            }

            AggregationOperatorType aot2 = aot ?? throw new NotImplementedException($"Don't know that aggregation {context.GetText()}");
            var ac = new AggregateContext(aot2, x);
            return ac;
        }
    }
}

