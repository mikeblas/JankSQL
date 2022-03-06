using Antlr4.Runtime.Misc;

namespace JankSQL
{

    public partial class JankListener : TSqlParserBaseListener
    {
        DeleteContext? deleteContext;

        public override void EnterDelete_statement_from([NotNull] TSqlParser.Delete_statement_fromContext context)
        {
            base.EnterDelete_statement_from(context);

            FullTableName tableName = FullTableName.FromFullTableNameContext(context.ddl_object().full_table_name());
            this.deleteContext = new DeleteContext(tableName);

            currentExpressionListList = new();
        }

        public override void ExitDelete_statement([NotNull] TSqlParser.Delete_statementContext context)
        {
            base.ExitDelete_statement(context);


        }
    }
}
