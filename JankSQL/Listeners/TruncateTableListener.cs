using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {
        public override void ExitTruncate_table([NotNull] TSqlParser.Truncate_tableContext context)
        {
            base.ExitTruncate_table(context);

            string tableName = context.table_name().id_()[0].GetText();

            TruncateTableContext c = new TruncateTableContext(tableName);
            executionContext.ExecuteContexts.Add(c);

        }
    }
}
