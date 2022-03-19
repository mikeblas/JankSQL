﻿
using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {

        internal AggregateContext GobbleAggregateFunctionContext(TSqlParser.Aggregate_windowed_functionContext context)
        {
            AggregateContext ac;

            if (context.SUM() != null)
            {
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.SUM, x);
            }
            else if (context.AVG() != null)
            {
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.AVG, x);
            }
            else if (context.MIN() != null)
            {
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.MIN, x);
            }
            else if (context.MAX() != null)
            {
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.MAX, x);
            }
            else if (context.STDEV() != null)
            {
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.STDEV, x);
            }
            else if (context.STDEVP() != null)
            {
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.STDEVP, x);
            }
            else if (context.VAR() != null)
            {
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.VAR, x);
            }
            else if (context.VARP() != null)
            {
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.VARP, x);
            }
            else if (context.COUNT() != null)
            {
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.COUNT, x);
            }
            else if (context.COUNT_BIG() != null)
            {
                Expression x = GobbleExpression(context.all_distinct_expression().expression());
                ac = new AggregateContext(AggregationOperatorType.COUNT_BIG, x);
            }
            else
            {
                throw new NotImplementedException($"Don't know that aggregation {context.GetText()}");
            }

            return ac;
        }
    }
}


