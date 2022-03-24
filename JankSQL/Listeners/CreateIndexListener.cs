namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;

    public partial class JankListener : TSqlParserBaseListener
    {
        public override void EnterCreate_index([NotNull] TSqlParser.Create_indexContext context)
        {
            base.EnterCreate_index(context);

            bool isUnique = context.UNIQUE() != null;
            string indexName = context.id_(0).GetText();
            var tableName = FullTableName.FromTableNameContext(context.table_name());

            Console.WriteLine($"create {isUnique} index named {indexName} on {tableName}");

            /*
            for (int i = 0; i < context.column_name_list_with_order().id_().Length; i++)
            {
                bool isDescending = context.column_name_list_with_order().DESC()[i] != null;
                string columnName = context.column_name_list_with_order().id_(i).ID().GetText();

                Console.WriteLine($"{columnName} {isDescending}");
            }
            */

            bool isDescending = false;
            string? columnName = null;
            for (int i = 0; i < context.column_name_list_with_order().ChildCount; i++)
            {
                var n = context.column_name_list_with_order().children[i];
                if (n is TerminalNodeImpl)
                {
                    if (n.ToString() == ",")
                    {
                        Console.WriteLine($"Got comma! {isDescending}, {columnName}");
                        isDescending = false;
                        columnName = null;
                    }
                    else if (n.ToString() == "ASC")
                    {
                        Console.WriteLine("ASC!");
                    }
                    else if (n.ToString() == "DESC")
                    {
                        Console.WriteLine("DESC!");
                        isDescending = true;
                    }
                }
                else if (n is TSqlParser.Id_Context idContext)
                {

                    Console.WriteLine($"ID: {idContext.ID()}");
                    columnName = idContext.ID().ToString();
                }
            }
            Console.WriteLine($"Finally! {isDescending}, {columnName}");
        }
    }
}
