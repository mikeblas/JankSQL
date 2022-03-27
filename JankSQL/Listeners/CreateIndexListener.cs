namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;
    using JankSQL.Contexts;

    public partial class JankListener : TSqlParserBaseListener
    {
        public override void EnterCreate_index([NotNull] TSqlParser.Create_indexContext context)
        {
            base.EnterCreate_index(context);

            bool isUnique = context.UNIQUE() != null;
            string indexName = context.id_(0).GetText();
            var tableName = FullTableName.FromTableNameContext(context.table_name());

            CreateIndexContext cic = new (tableName, indexName, isUnique);

            Console.WriteLine($"create {isUnique} index named {indexName} on {tableName}");

            bool isDescending = false;
            string? columnName = null;
            for (int i = 0; i < context.column_name_list_with_order().ChildCount; i++)
            {
                var n = context.column_name_list_with_order().children[i];
                if (n is TerminalNodeImpl)
                {
                    if (n.ToString() == ",")
                    {
                        Console.WriteLine($"Got comma! {(isDescending ? "DESC" : "ASC")}, {columnName}");

                        cic.AddColumn(columnName, isDescending);
                        isDescending = false;
                        columnName = null;
                    }
                    else if (n.ToString() == "ASC")
                        isDescending = false;
                    else if (n.ToString() == "DESC")
                        isDescending = true;
                }
                else if (n is TSqlParser.Id_Context idContext)
                {

                    Console.WriteLine($"ID: {idContext.ID()}");
                    columnName = idContext.ID().ToString();
                }
            }

            if (columnName != null)
            {
                Console.WriteLine($"Finally! {(isDescending ? "DESC" : "ASC")}, {columnName}");
                cic.AddColumn(columnName, isDescending);
            }

            executionContext.ExecuteContexts.Add(cic);
        }
    }
}
