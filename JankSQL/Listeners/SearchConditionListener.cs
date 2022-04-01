namespace JankSQL
{
    using JankSQL.Expressions;

    public partial class JankListener : TSqlParserBaseListener
    {
        internal Expression GobbleSearchCondition(TSqlParser.Search_conditionContext context)
        {
            Expression x = new ();

            if (context.OR() != null)
            {
                Expression left = GobbleSearchCondition(context.search_condition()[0]);
                Expression right = GobbleSearchCondition(context.search_condition()[1]);

                ExpressionNode op = ExpressionBooleanOperator.GetOrOperator();

                x.AddRange(left);
                x.AddRange(right);
                x.Add(op);
            }
            else if (context.AND() != null)
            {
                Expression left = GobbleSearchCondition(context.search_condition()[0]);
                Expression right = GobbleSearchCondition(context.search_condition()[1]);

                ExpressionNode op = ExpressionBooleanOperator.GetAndOperator();

                x.AddRange(left);
                x.AddRange(right);
                x.Add(op);
            }
            else if (context.predicate() != null)
            {
                Expression operand;
                if (context.predicate() != null)
                {
                    operand = GobblePredicate(context.predicate());
                }
                else if (context.search_condition() != null)
                {
                    operand = GobbleSearchCondition(context.search_condition()[0]);
                }
                else
                {
                    throw new InvalidOperationException("How did I get here?");
                }

                x.AddRange(operand);

                for (int i = 0; i < context.NOT().Length; i++)
                {
                    ExpressionNode op = ExpressionBooleanOperator.GetNotOperator();
                    x.Add(op);
                }
            }
            else if (context.LR_BRACKET() != null || context.RR_BRACKET() != null)
            {
                if (context.search_condition().Length != 1)
                {
                    throw new InvalidOperationException("Can't cope with search_condition length != 1");
                }

                x = GobbleSearchCondition(context.search_condition()[0]);

                for (int i = 0; i < context.NOT().Length; i++)
                {
                    ExpressionNode op = ExpressionBooleanOperator.GetNotOperator();
                    x.Add(op);
                }
            }
            else
            {
                throw new InvalidOperationException("How did I get here?");
            }

            return x;
        }

        internal Expression GobblePredicate(TSqlParser.PredicateContext context)
        {
            ExpressionNode comparison = new ExpressionComparisonOperator(context.comparison_operator().GetText());

            Expression left = GobbleExpression(context.expression()[0]);
            Expression right = GobbleExpression(context.expression()[1]);

            Expression x = new ();
            x.AddRange(left);
            x.AddRange(right);
            x.Add(comparison);

            return x;
        }
    }
}
