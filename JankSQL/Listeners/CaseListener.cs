namespace JankSQL
{
    using JankSQL.Contexts;
    using JankSQL.Expressions;

    public partial class JankListener : TSqlParserBaseListener
    {
        internal ExpressionNode GobbleCaseExpression(TSqlParser.Case_expressionContext context)
        {
            if (context.switch_search_condition_section().Length > 0)
                return GobbleSearchedCaseExpression(context);
            else
                return GobbleSimpleCaseExpression(context);
        }

        internal ExpressionNode GobbleSearchedCaseExpression(TSqlParser.Case_expressionContext context)
        {
            var searchExpression = context.switch_search_condition_section();

            List<Expression> whens = new();
            List<Expression> thens = new();

            for (int i = 0; i < searchExpression.Length; i++)
            {
                whens.Add(GobbleSearchCondition(searchExpression[i].search_condition()));
                thens.Add(GobbleExpression(searchExpression[i].expression()));
            }

            Expression? elseExpression = null;
            if (context.ELSE != null)
                elseExpression = GobbleExpression(context.elseExpr);

            Console.WriteLine($"{string.Join(", ", whens)}");
            Console.WriteLine($"{string.Join(", ", thens)}");
            Console.WriteLine($"{(elseExpression == null ? "no else" : elseExpression.ToString())}");

            return new ExpressionCaseOperator(whens, thens, elseExpression);
        }

        internal ExpressionNode GobbleSimpleCaseExpression(TSqlParser.Case_expressionContext context)
        {
            var switchSection = context.switch_section();

            // the input expression
            Expression inputExpression = GobbleExpression(context.expression()[0]);

            List<Expression> whens = new();
            List<Expression> thens = new();

            for (int i = 0; i < switchSection.Length; i++)
            {
                Expression when = GobbleExpression(switchSection[i].expression()[0]);
                when.AddRange(inputExpression);
                when.Add(new ExpressionComparisonOperator("="));

                whens.Add(when);

                thens.Add(GobbleExpression(switchSection[i].expression()[1]));
            }

            Expression? elseExpression = null;
            if (context.ELSE != null)
                elseExpression = GobbleExpression(context.elseExpr);

            Console.WriteLine($"{string.Join("; ", whens)}");
            Console.WriteLine($"{string.Join("; ", thens)}");
            Console.WriteLine($"{(elseExpression == null ? "no else" : elseExpression.ToString())}");

            return new ExpressionCaseOperator(whens, thens, elseExpression);
        }

    }
}
