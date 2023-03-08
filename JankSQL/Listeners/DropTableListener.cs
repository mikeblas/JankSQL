namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;

    public partial class JankListener : TSqlParserBaseListener
    {
        public override void ExitDrop_table([NotNull] TSqlParser.Drop_tableContext context)
        {
            base.ExitDrop_table(context);

            //TODO: handle list of tables
            FullTableName tableName = FullTableName.FromTableNameContext(context.table_name()[0]);
            var dtc = new DropTableContext(tableName);

            executionContext.ExecuteContexts.Add(dtc);
        }
    }
}
