namespace JankSQL
{
    using JankSQL.Contexts;
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
                    throw new InvalidOperationException("Can't cope with search_condition length != 1");

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
            if (context.IN() != null)
            {
                // expression [NOT] IN (subquery | expression_list)
                // throw new NotImplementedException("subqueries are not yet supported");

                Console.WriteLine("IN clause Predicate Expression!");

                Expression left = GobbleExpression(context.expression()[0]);

                // there are a variable number of NOT tokens
                // if that number is odd, then we are NOT IN,
                // otherwise, IN
                bool notIn = context.NOT().Length % 2 != 0;

                if (context.expression_list() != null)
                {
                    List<Expression> expressions = new ();

                    foreach (var expression in context.expression_list().expression())
                    {
                        Expression expr = GobbleExpression(expression);
                        expressions.Add(expr);
                        Console.WriteLine($":  {expr}");
                    }

                    var oper = new ExpressionInOperator(notIn, expressions);

                    Expression x = new ();
                    x.AddRange(left);
                    x.Add(oper);

                    return x;
                }
                else if (context.subquery() != null)
                {
                    // it's a subselect
                    SelectContext selectContext = GobbleSelectStatement(context.subquery().select_statement());

                    var oper = new ExpressionInOperator(notIn, selectContext);

                    Expression x = new ();
                    x.AddRange(left);
                    x.Add(oper);

                    return x;
                }
            }
            else if (context.comparison_operator() != null)
            {
                // two expressions with a comparison operator in the middle
                var comparison = new ExpressionComparisonOperator(context.comparison_operator().GetText());

                Expression left = GobbleExpression(context.expression()[0]);
                Expression right = GobbleExpression(context.expression()[1]);

                Expression x = new ();
                x.AddRange(left);
                x.AddRange(right);
                x.Add(comparison);

                return x;
            }
            else if (context.null_notnull() != null)
            {
                // expression IS [NOT] NULL
                var comparison = new ExpressionIsNullOperator(context.null_notnull().NOT() != null);

                Expression left = GobbleExpression(context.expression()[0]);

                Expression x = new ();
                x.AddRange(left);
                x.Add(comparison);

                return x;
            }
            else if (context.BETWEEN() != null)
            {
                // expression [NOT] BETWEEN expresion AND expression

                Expression value = GobbleExpression(context.expression()[0]);
                Expression left = GobbleExpression(context.expression()[1]);
                Expression right = GobbleExpression(context.expression()[2]);

                // there are a variable number of NOT tokens
                // if that number is odd, then we are NOT BETWEEN
                // otherwise, BETWEEN
                bool notBetween = context.NOT().Length % 2 != 0;

                var comparison = new ExpressionBetweenOperator(notBetween);

                Expression x = new ();
                x.AddRange(value);
                x.AddRange(left);
                x.AddRange(right);
                x.Add(comparison);

                return x;
            }

            throw new InternalErrorException("Don't know how to handle this predicate");
        }
    }
}
