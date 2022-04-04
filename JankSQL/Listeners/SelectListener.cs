namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;
    using JankSQL.Expressions;

    public partial class JankListener : TSqlParserBaseListener
    {
        public override void ExitSelect_statement(TSqlParser.Select_statementContext context)
        {
            base.ExitEveryRule(context);
        }

        public override void EnterSelect_statement([NotNull] TSqlParser.Select_statementContext context)
        {
            base.EnterSelect_statement(context);

            PredicateContext? pc = null;

            // consume the WHERE predicate
            if (context.query_expression().query_specification().search_condition().Length > 0)
            {
                Expression x = GobbleSearchCondition(context.query_expression().query_specification().search_condition()[0]);
                pc = new PredicateContext(x);
            }

            SelectContext selectContext = new (context, pc);

            // get through the select list
            foreach (var elem in context.query_expression().query_specification().select_list().select_list_elem())
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

            // see if there is an order-by clause
            if (context.order_by_clause() != null)
            {
                OrderByContext obc = new ();

                foreach (var expr in context.order_by_clause().order_by_expression())
                {
                    Expression obx = GobbleExpression(expr.expression());
                    Console.Write($"   {string.Join(",", obx.Select(x => $"[{x}]"))} ");
                    if (expr.DESC() != null)
                        Console.WriteLine("DESC");
                    else
                        Console.WriteLine("ASC");

                    obc.AddExpression(obx, expr.DESC() == null);
                }

                selectContext.OrderByContext = obc;
            }

            // and any group-by clauses
            foreach (var groupByItem in context.query_expression().query_specification().group_by_item())
            {
                Expression gbe = GobbleExpression(groupByItem.expression());
                selectContext.AddGroupByExpression(gbe);
            }

            // figure out sources
            if (context.query_expression().query_specification().table_sources() == null)
            {
                // no table source, so it's just from the "DUAL" table
            }
            else
            {
                // find the source table, along with all the joins

                var talbeSourceItem = context.query_expression().query_specification().table_sources().table_source().First().table_source_item_joined();
                while (talbeSourceItem != null)
                {
                    FullTableName ftn = FullTableName.FromTableNameContext(talbeSourceItem.table_source_item().table_name_with_hint().table_name());
                    Console.WriteLine($"iterative: {ftn}");

                    if (selectContext.SourceTableName == null)
                        selectContext.SourceTableName = ftn;

                    if (talbeSourceItem.join_part().Length > 0)
                    {
                        var joinContext = talbeSourceItem.join_part()[0];
                        if (joinContext == null)
                            talbeSourceItem = null;
                        else
                        {
                            // x2 = j.cross_join().table_source().table_source_item_joined();
                            // figure out which join type
                            if (joinContext.cross_join() != null)
                            {
                                // CROSS Join!

                                FullTableName otherTableName = FullTableName.FromTableNameContext(joinContext.cross_join().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name());
                                Console.WriteLine($"{ftn} CROSS JOIN On {otherTableName}");

                                JoinContext jc = new (JoinType.CROSS_JOIN, otherTableName);
                                PredicateContext pcon = new ();
                                selectContext.AddJoin(jc, pcon);

                                talbeSourceItem = joinContext.cross_join().table_source().table_source_item_joined();
                            }
                            else if (joinContext.join_on() != null)
                            {
                                Expression x = GobbleSearchCondition(joinContext.join_on().search_condition());
                                PredicateContext pcon = new (x);

                                // ON join
                                FullTableName otherTableName = FullTableName.FromTableNameContext(joinContext.join_on().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name());
                                Console.WriteLine($"INNER JOIN On {otherTableName}");

                                JoinContext jc = new (JoinType.INNER_JOIN, otherTableName);
                                selectContext.AddJoin(jc, pcon);

                                talbeSourceItem = joinContext.join_on().table_source().table_source_item_joined();
                            }
                            else
                            {
                                throw new NotImplementedException("unsupported JOIN type enountered");
                            }

                        }
                    }
                    else
                        talbeSourceItem = null;
                }
            }


            executionContext.ExecuteContexts.Add(selectContext);

        }

        public override void EnterSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.EnterSelect_list(context);
        }

        public override void ExitSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.ExitSelect_list(context);
        }

    }
}
