namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;

    public partial class JankListener : TSqlParserBaseListener
    {
        private DeleteContext? deleteContext;

        public override void EnterDelete_statement_from([NotNull] TSqlParser.Delete_statement_fromContext context)
        {
            base.EnterDelete_statement_from(context);
        }

        public override void EnterDelete_statement([NotNull] TSqlParser.Delete_statementContext context)
        {
            base.EnterDelete_statement(context);

            FullTableName tableName = FullTableName.FromFullTableNameContext(context.delete_statement_from().ddl_object().full_table_name());
            this.deleteContext = new DeleteContext(tableName);

            if (deleteContext == null)
                throw new InternalErrorException("Expected a DeleteContext");

            // see if there's a search condition
            if (context.search_condition() != null)
            {
                Expression pred = GobbleSearchCondition(context.search_condition());
                deleteContext.PredicateExpression = pred;
            }

            executionContext.ExecuteContexts.Add(deleteContext);
        }

        public override void ExitDelete_statement([NotNull] TSqlParser.Delete_statementContext context)
        {
            base.ExitDelete_statement(context);

            if (deleteContext == null)
                throw new InternalErrorException("Expected a DeleteContext");

            executionContext.ExecuteContexts.Add(deleteContext);
            deleteContext = null;
        }
    }
}
