
using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {

        internal Aggregation GobbleAggregateFunctionContext(TSqlParser.Aggregate_windowed_functionContext context)
        {
            Aggregation agg;

            if (context.SUM() != null)
            {
                Console.WriteLine("got SUM");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new Aggregation(AggregationOperatorType.SUM, x);
            }
            else if (context.AVG() != null)
            {
                Console.WriteLine("got AVG");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new Aggregation(AggregationOperatorType.AVG, x);
            }
            else if (context.MIN() != null)
            {
                Console.WriteLine("got MIN");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new Aggregation(AggregationOperatorType.MIN, x);
            }
            else if (context.MAX() != null)
            {
                Console.WriteLine("got MAX");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new Aggregation(AggregationOperatorType.MAX, x);
            }
            else if (context.STDEV() != null)
            {
                Console.WriteLine("got STDEV");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new Aggregation(AggregationOperatorType.STDEV, x);
            }
            else if (context.STDEVP() != null)
            {
                Console.WriteLine("got STDEVP");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new Aggregation(AggregationOperatorType.STDEVP, x);
            }
            else if (context.VAR() != null)
            {
                Console.WriteLine("got VAR");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new Aggregation(AggregationOperatorType.VAR, x);
            }
            else if (context.VARP() != null)
            {
                Console.WriteLine("got VARP");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new Aggregation(AggregationOperatorType.VARP, x);
            }
            else if (context.COUNT() != null)
            {
                Console.WriteLine("got COUNT");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new Aggregation(AggregationOperatorType.COUNT, x);
            }
            else if (context.COUNT_BIG() != null)
            {
                Console.WriteLine("got COUNT_BIG");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                agg = new Aggregation(AggregationOperatorType.COUNT_BIG, x);
            }
            else
            {
                throw new NotImplementedException($"Don't know that aggregation {context.GetText()}");
            }

            return agg;
        }
    }
}


