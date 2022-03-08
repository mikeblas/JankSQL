using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {
        InsertContext? insertContext;

        public override void EnterInsert_statement([NotNull] TSqlParser.Insert_statementContext context)
        {
            base.EnterInsert_statement(context);

            insertContext = new InsertContext(context);
        }


        public override void ExitInsert_column_name_list([NotNull] TSqlParser.Insert_column_name_listContext context)
        {
            base.ExitInsert_column_name_list(context);

            if (insertContext == null)
                throw new InternalErrorException("Expected an InsertContext");

            List<FullColumnName> columns = new();

            foreach (var col in context.insert_column_id())
            {
                Console.WriteLine(col.id_()[0].GetText());
                columns.Add(FullColumnName.FromColumnName(col.id_()[0].GetText()));
            }

            insertContext.TargetColumns = columns;
        }


        public override void ExitInsert_statement([NotNull] TSqlParser.Insert_statementContext context)
        {
            base.ExitInsert_statement(context);

            if (insertContext == null)
                throw new InternalErrorException("Expected an InsertContext");

            insertContext.TableName = context.ddl_object().full_table_name().GetText();
            Console.WriteLine($"INTO {insertContext.TableName}");

            executionContext.ExecuteContexts.Add(insertContext);
        }


        public override void ExitInsert_with_table_hints([NotNull] TSqlParser.Insert_with_table_hintsContext context)
        {
            base.ExitInsert_with_table_hints(context);
        }


        public override void EnterTable_value_constructor([NotNull] TSqlParser.Table_value_constructorContext context)
        {
            base.EnterTable_value_constructor(context);

            currentExpressionListList = new();
            currentExpressionList = new();
        }

        public override void ExitTable_value_constructor([NotNull] TSqlParser.Table_value_constructorContext context)
        {
            base.ExitTable_value_constructor(context);

            if (insertContext == null)
                throw new InternalErrorException("Expected an InsertContext");

            insertContext.AddExpressionLists(currentExpressionListList);
            currentExpressionList = new();
        }
    }
}

