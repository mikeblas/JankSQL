using Antlr4.Runtime.Misc;
using JankSQL.Contexts;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {
        public override void ExitTruncate_table([NotNull] TSqlParser.Truncate_tableContext context)
        {
            base.ExitTruncate_table(context);

            FullTableName ftn = FullTableName.FromTableNameContext(context.table_name());

            TruncateTableContext c = new TruncateTableContext(ftn);
            executionContext.ExecuteContexts.Add(c);

        }
    }
}
