namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;

    public partial class JankListener : TSqlParserBaseListener
    {
        public override void EnterTransaction_statement([NotNull] TSqlParser.Transaction_statementContext context)
        {
            base.EnterTransaction_statement(context);

            if (context.COMMIT != null)
            {
                CommitContext ctx = new ();
                executionContext.ExecuteContexts.Add(ctx);
            }
            else if (context.ROLLBACK != null)
            {
                RollbackContext ctx = new ();
                executionContext.ExecuteContexts.Add(ctx);
            }
        }
    }
}

