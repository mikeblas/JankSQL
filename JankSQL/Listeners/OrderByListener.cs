namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;

    public partial class JankListener : TSqlParserBaseListener
    {

        public override void EnterOrder_by_clause([NotNull] TSqlParser.Order_by_clauseContext context)
        {
            base.EnterOrder_by_clause(context);

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");

            OrderByContext obc = new ();

            foreach (var expr in context.order_by_expression())
            {
                Expression obx = GobbleExpression(expr.expression());
                Console.Write($"   {string.Join(",", obx.Select(x => $"[{x}]"))} ");
                if (expr.DESC() != null)
                    Console.WriteLine("DESC");
                else
                    Console.WriteLine("ASC");

                obc.AddExpression(obx, expr.DESC() == null);
            }

            selectContext.OrderByContext = obc;
        }
    }
}
