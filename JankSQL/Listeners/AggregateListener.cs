
using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {

        internal AggregateContext GobbleAggregateFunctionContext(TSqlParser.Aggregate_windowed_functionContext context)
        {
            AggregateContext agg;

            if (context.SUM() != null)
            {
                Console.WriteLine("got SUM");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new AggregateContext(AggregationOperatorType.SUM, x);
            }
            else if (context.AVG() != null)
            {
                Console.WriteLine("got AVG");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new AggregateContext(AggregationOperatorType.AVG, x);
            }
            else if (context.MIN() != null)
            {
                Console.WriteLine("got MIN");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new AggregateContext(AggregationOperatorType.MIN, x);
            }
            else if (context.MAX() != null)
            {
                Console.WriteLine("got MAX");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new AggregateContext(AggregationOperatorType.MAX, x);
            }
            else if (context.STDEV() != null)
            {
                Console.WriteLine("got STDEV");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new AggregateContext(AggregationOperatorType.STDEV, x);
            }
            else if (context.STDEVP() != null)
            {
                Console.WriteLine("got STDEVP");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new AggregateContext(AggregationOperatorType.STDEVP, x);
            }
            else if (context.VAR() != null)
            {
                Console.WriteLine("got VAR");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new AggregateContext(AggregationOperatorType.VAR, x);
            }
            else if (context.VARP() != null)
            {
                Console.WriteLine("got VARP");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new AggregateContext(AggregationOperatorType.VARP, x);
            }
            else if (context.COUNT() != null)
            {
                Console.WriteLine("got COUNT");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new AggregateContext(AggregationOperatorType.COUNT, x);
            }
            else if (context.COUNT_BIG() != null)
            {
                Console.WriteLine("got COUNT_BIG");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new AggregateContext(AggregationOperatorType.COUNT_BIG, x);
            }
            else
            {
                throw new NotImplementedException($"Don't know that aggregation {context.GetText()}");
            }

            return agg;
        }
    }
}


