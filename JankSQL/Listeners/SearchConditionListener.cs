using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {
        public override void EnterSearch_condition([NotNull] TSqlParser.Search_conditionContext context)
        {
            base.EnterSearch_condition(context);
            currentExpressionList = new();
        }

        public override void ExitSearch_condition([NotNull] TSqlParser.Search_conditionContext context)
        {
            base.ExitSearch_condition(context);

            if (predicateContext == null)
                throw new InternalErrorException("Expected a PredicateContext");


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
        }

    }
}
