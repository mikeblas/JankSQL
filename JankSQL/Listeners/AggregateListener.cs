
using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {

        internal AggregateContext GobbleAggregateFunctionContext(TSqlParser.Aggregate_windowed_functionContext context, int expressionID)
        {
            AggregateContext ac;

            if (context.SUM() != null)
            {
                Console.WriteLine("got SUM");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.SUM, x, expressionID);
            }
            else if (context.AVG() != null)
            {
                Console.WriteLine("got AVG");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.AVG, x, expressionID);
            }
            else if (context.MIN() != null)
            {
                Console.WriteLine("got MIN");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.MIN, x, expressionID);
            }
            else if (context.MAX() != null)
            {
                Console.WriteLine("got MAX");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.MAX, x, expressionID);
            }
            else if (context.STDEV() != null)
            {
                Console.WriteLine("got STDEV");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.STDEV, x, expressionID);
            }
            else if (context.STDEVP() != null)
            {
                Console.WriteLine("got STDEVP");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.STDEVP, x, expressionID);
            }
            else if (context.VAR() != null)
            {
                Console.WriteLine("got VAR");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.VAR, x, expressionID);
            }
            else if (context.VARP() != null)
            {
                Console.WriteLine("got VARP");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.VARP, x, expressionID);
            }
            else if (context.COUNT() != null)
            {
                Console.WriteLine("got COUNT");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.COUNT, x, expressionID);
            }
            else if (context.COUNT_BIG() != null)
            {
                Console.WriteLine("got COUNT_BIG");
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.COUNT_BIG, x, expressionID);
            }
            else
            {
                throw new NotImplementedException($"Don't know that aggregation {context.GetText()}");
            }

            return ac;
        }
    }
}


