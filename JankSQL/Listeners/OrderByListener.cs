namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;

    public partial class JankListener : TSqlParserBaseListener
    {
        public override void EnterOrder_by_clause([NotNull] TSqlParser.Order_by_clauseContext context)
        {
            base.EnterOrder_by_clause(context);

        }
    }
}
