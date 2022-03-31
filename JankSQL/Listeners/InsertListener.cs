namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;

    public partial class JankListener : TSqlParserBaseListener
    {
        private InsertContext? insertContext;

        public override void EnterInsert_statement([NotNull] TSqlParser.Insert_statementContext context)
        {
            base.EnterInsert_statement(context);

            insertContext = new InsertContext(context, FullTableName.FromFullTableNameContext(context.ddl_object().full_table_name()));
        }


        public override void ExitInsert_column_name_list([NotNull] TSqlParser.Insert_column_name_listContext context)
        {
            base.ExitInsert_column_name_list(context);

            if (insertContext == null)
                throw new InternalErrorException("Expected an InsertContext");

            List<FullColumnName> columns = new ();

            foreach (var col in context.insert_column_id())
                columns.Add(FullColumnName.FromColumnName(col.id_()[0].GetText()));

            insertContext.TargetColumns = columns;

            // check for repeated column names in the insert list
            HashSet<FullColumnName> names = new ();
            for (int i = 0; i < columns.Count; i++)
            {
                if (names.Contains(columns[i]))
                    throw new ExecutionException($"column {columns[i]} appears in insert list more than once");
                names.Add(columns[i]);
            }
        }

        public override void ExitInsert_statement([NotNull] TSqlParser.Insert_statementContext context)
        {
            base.ExitInsert_statement(context);

            if (insertContext == null)
                throw new InternalErrorException("Expected an InsertContext");

            executionContext.ExecuteContexts.Add(insertContext);
        }


        public override void ExitInsert_with_table_hints([NotNull] TSqlParser.Insert_with_table_hintsContext context)
        {
            base.ExitInsert_with_table_hints(context);
        }


        public override void EnterTable_value_constructor([NotNull] TSqlParser.Table_value_constructorContext context)
        {
            base.EnterTable_value_constructor(context);

            if (insertContext == null)
                throw new InternalErrorException("Expected an InsertContext");


            List<List<Expression>> total = new ();

            int? constructorColumns = null;

            foreach (var expressionList in context.expression_list())
            {
                List<Expression> constructor = new ();
                foreach (var expr in expressionList.expression())
                {
                    Expression x = GobbleExpression(expr);
                    constructor.Add(x);
                }

                if (constructorColumns == null)
                    constructorColumns = constructor.Count;
                else
                {
                    if (constructorColumns != constructor.Count)
                        throw new ExecutionException($"constructors should have {constructorColumns} columns, found {constructor.Count}");
                }

                total.Add(constructor);
            }

            insertContext.AddExpressionLists(total);
        }

    }
}

