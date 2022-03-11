using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {
        public override void EnterQuery_specification([NotNull] TSqlParser.Query_specificationContext context)
        {
            base.EnterQuery_specification(context);

            if (context.search_condition().Length > 0)
            {
                Expression x = GobbleSearchCondition(context.search_condition()[0]);

                predicateContext = new();
                predicateContext.EndPredicateExpressionList(x);
            }
            /*
            // predicateContext = new();
            // predicateContext.EndPredicateExpressionList(x);
            */
        }


        public override void EnterSearch_condition([NotNull] TSqlParser.Search_conditionContext context)
        {
            base.EnterSearch_condition(context);
            /*
            currentExpressionList = new();
            currentExpressionListList = new();
            */

            /*
            Expression x = GobbleSearchCondition(context);

            predicateContext = new();
            predicateContext.EndPredicateExpressionList(x);
            */
        }

        Expression GobbleSearchCondition(TSqlParser.Search_conditionContext context)
        {
            Expression x = new();

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


        Expression GobblePredicate(TSqlParser.PredicateContext context)
        {
            ExpressionNode comparison = new ExpressionComparisonOperator(context.comparison_operator().GetText());

            Expression left = GobbleExpression(context.expression()[0]);
            Expression right = GobbleExpression(context.expression()[1]);

            Expression x = new();
            x.AddRange(left);
            x.AddRange(right);
            x.Add(comparison);

            return x;
        }


        public override void ExitSearch_condition([NotNull] TSqlParser.Search_conditionContext context)
        {
            base.ExitSearch_condition(context);

            /*

            if (predicateContext == null)
                throw new InternalErrorException("Expected a PredicateContext");
            if (currentExpressionListList == null)
                throw new InternalErrorException("Expected a ExpressionListList");
            if (currentExpressionList == null)
                throw new InternalErrorException("Expected a ExpressionList");

            Expression total = new();
            foreach (var l in currentExpressionListList)
            {
                foreach (var x in l)
                {
                    total.AddRange(x);
                }
            }

            foreach (var x in currentExpressionList)
                total.AddRange(x);
            currentExpressionList = new();
            currentExpression = new();

            if (context.AND() != null)
            {
                Console.WriteLine("Got AND");
                ExpressionNode x = ExpressionBooleanOperator.GetAndOperator();
                total.Add(x);
                predicateContext.EndAndCombinePredicateExpressionList(2, total);
            }
            else if (context.OR() != null)
            {
                Console.WriteLine("Got OR");
                ExpressionNode x = ExpressionBooleanOperator.GetOrOperator();
                total.Add(x);
                predicateContext.EndAndCombinePredicateExpressionList(2, total);
            }
            else if (context.NOT(0) != null)
            {
                int n = 0;
                do
                {
                    Console.WriteLine("Got NOT");
                    ExpressionNode x = ExpressionBooleanOperator.GetNotOperator();
                    total.Add(x);
                    if (total.Count == 1)
                    {
                        predicateContext.EndAndCombinePredicateExpressionList(1, total);
                    }
                    else
                    {
                        predicateContext.EndPredicateExpressionList(total);
                    }
                } while (context.NOT(++n) != null);
            }
            else
            {
                Console.WriteLine("Got neither");
                predicateContext.EndPredicateExpressionList(total);
            }
            */
        }

    }
}
