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

            Expression x = GobbleSearchCondition(context.search_condition());

            deleteContext.PredicateContext = new PredicateContext(x);
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
