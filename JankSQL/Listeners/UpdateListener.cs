

using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {
        UpdateContext? updateContext;

        public override void EnterUpdate_statement([NotNull] TSqlParser.Update_statementContext context)
        {
            base.EnterUpdate_statement(context);

            updateContext = new UpdateContext(context, FullTableName.FromFullTableNameContext(context.ddl_object().full_table_name()));
            Console.WriteLine($"UPDATE {updateContext.TableName}");

            predicateContext = new();

        }

        public override void EnterUpdate_elem([NotNull] TSqlParser.Update_elemContext context)
        {
            base.EnterUpdate_elem(context);

        }

        public override void ExitUpdate_elem([NotNull] TSqlParser.Update_elemContext context)
        {
            base.ExitUpdate_elem(context);

            // Console.WriteLine($"Update Element: {String.Join(",", currentExpressionList)}");

            /*
            if (context.Equals != null)
                Console.WriteLine("Update Element Operator: Explicit EQUALS");
            else
                Console.WriteLine($"Update Element Operator: {context.assignment_operator().GetText()}");
            */
        }

        public override void ExitUpdate_statement([NotNull] TSqlParser.Update_statementContext context)
        {
            base.ExitUpdate_statement(context);

            if (updateContext == null)
                throw new InternalErrorException("Expected an UpdateContext");
            if (predicateContext == null)
                throw new InternalErrorException("Expected a PredicateContext");

            updateContext.PredicateContext = predicateContext;
            predicateContext = null;

            executionContext.ExecuteContexts.Add(updateContext);
            updateContext = null;
        }
    }
}
