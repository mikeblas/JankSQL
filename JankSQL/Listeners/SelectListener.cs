namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;
    using JankSQL.Expressions;

    public partial class JankListener : TSqlParserBaseListener
    {
        private SelectContext? selectContext;

        public override void ExitSelect_statement(TSqlParser.Select_statementContext context)
        {
            base.ExitEveryRule(context);

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");

            executionContext.ExecuteContexts.Add(selectContext);
        }

        public override void EnterSelect_statement([NotNull] TSqlParser.Select_statementContext context)
        {
            base.EnterSelect_statement(context);

            PredicateContext? pc = null;

            if (context.query_expression().query_specification().search_condition().Length > 0)
            {
                Expression x = GobbleSearchCondition(context.query_expression().query_specification().search_condition()[0]);
                pc = new PredicateContext(x);
            }

            selectContext = new SelectContext(context, pc);
        }

        public override void EnterSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.EnterSelect_list(context);

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");

            selectContext.SelectListContext = new SelectListContext(context);
        }

        public override void ExitSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.ExitSelect_list(context);

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");
            if (selectContext.SelectListContext == null)
                throw new InternalErrorException("Expected a SelectListContext");

            foreach (var elem in context.select_list_elem())
            {
                FullColumnName? fcn = null;
                Expression? x;

                if (elem.column_elem() != null)
                {
                    // NULL in a select list is a column element, not an expression element
                    if (elem.column_elem().NULL_() != null)
                    {
                        ExpressionNode n = ExpressionOperand.NullLiteral();
                        x = new () { n };
                    }
                    else
                    {
                        ExpressionNode n = new ExpressionOperandFromColumn(FullColumnName.FromContext(elem.column_elem().full_column_name()));
                        x = new () { n };

                        fcn = FullColumnName.FromContext(elem.column_elem().full_column_name());
                    }

                    // column elements have the AS clause here
                    if (elem.column_elem().as_column_alias() != null)
                        fcn = FullColumnName.FromColumnName(elem.column_elem().as_column_alias().column_alias().id_().GetText());
                }
                else if (elem.expression_elem() != null)
                {
                    if (elem.expression_elem().as_column_alias() != null)
                        fcn = FullColumnName.FromColumnName(elem.expression_elem().as_column_alias().GetText());

                    x = GobbleExpression(elem.expression_elem().expression());
                }
                else if (elem.asterisk() != null)
                {
                    Console.WriteLine("asterisk!");
                    continue;
                }
                else
                {
                    Console.WriteLine("Don't know this type");
                    continue;
                }

                if (fcn != null)
                    selectContext.SelectListContext.AddRowsetColumnName(fcn);
                else
                    selectContext.SelectListContext.AddUnknownRowsetColumnName();

                Console.WriteLine($"SelectListElement:   {string.Join(" ", x)}");
                selectContext.AddSelectListExpressionList(x);
            }
        }

    }
}
