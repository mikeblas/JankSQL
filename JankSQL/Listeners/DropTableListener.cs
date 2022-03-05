﻿using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {
        public override void ExitDrop_table([NotNull] TSqlParser.Drop_tableContext context)
        {
            base.ExitDrop_table(context);

            FullTableName tableName = FullTableName.FromTableNameContext(context.table_name());
            DropTableContext dtc = new DropTableContext(tableName);

            executionContext.ExecuteContexts.Add(dtc);
        }
    }
}
