using Antlr4.Runtime.Misc;

namespace JankSQL
{

    public partial class JankListener : TSqlParserBaseListener
    {
        DeleteContext? deleteContext;

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

            PredicateContext pcon = new PredicateContext();
            pcon.EndPredicateExpressionList(x);
            deleteContext.PredicateContext = pcon;
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
