namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;

    public partial class JankListener : TSqlParserBaseListener
    {
        private UpdateContext? updateContext;

        public override void EnterUpdate_statement([NotNull] TSqlParser.Update_statementContext context)
        {
            base.EnterUpdate_statement(context);

            updateContext = new UpdateContext(context, FullTableName.FromFullTableNameContext(context.ddl_object().full_table_name()));
            Console.WriteLine($"UPDATE {updateContext.TableName}");

            foreach (var element in context.update_elem())
            {
                FullColumnName fcn = FullColumnName.FromContext(element.full_column_name());
                Expression x = GobbleExpression(element.expression());

                Console.Write($"   SET {fcn} ");
                if (element.Equals != null)
                {
                    Console.Write("Equals ");
                    updateContext.AddAssignment(fcn, x);
                }
                else if (element.assignment_operator() != null)
                {
                    Console.Write("Assignment operator {element.assignment_operator().GetText()}");
                    updateContext.AddAssignmentOperator(fcn, element.assignment_operator().GetText(), x);
                }
                else
                {
                    throw new InvalidOperationException("unknown UPDATE structure");
                }

                Console.WriteLine($"{x}");
            }
        }


        public override void ExitUpdate_statement([NotNull] TSqlParser.Update_statementContext context)
        {
            base.ExitUpdate_statement(context);

            if (updateContext == null)
                throw new InternalErrorException("Expected an UpdateContext");

            Expression pred = GobbleSearchCondition(context.search_condition());

            updateContext.PredicateContext = new (pred);

            executionContext.ExecuteContexts.Add(updateContext);
            updateContext = null;
        }
    }
}

