namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;
    using JankSQL.Expressions;

    public partial class JankListener : TSqlParserBaseListener
    {
        public override void EnterSelect_statement_standalone([NotNull] TSqlParser.Select_statement_standaloneContext context)
        {
            base.EnterSelect_statement_standalone(context);

            SelectContext selectContext = GobbleSelectStatement(context.select_statement());

            executionContext.ExecuteContexts.Add(selectContext);
        }

        internal SelectContext GobbleSelectStatement(TSqlParser.Select_statementContext context)
        {
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

                    x = GobbleSelectExpression(elem.expression_elem().expression(), selectContext);
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

                var currentTSIJ = context.query_expression().query_specification().from.table_source()[0].table_source_item_joined();
                while (currentTSIJ != null)
                {
                    string leftSource = "unassigned";
                    if (selectContext.SourceTableName == null && selectContext.InputContext == null)
                    {
                        if (currentTSIJ.table_source_item().derived_table() != null)
                        {
                            SelectContext inner = GobbleSelectStatement(currentTSIJ.table_source_item().derived_table().subquery()[0].select_statement());
                            leftSource = "Subselect";
                            selectContext.InputContext = inner;
                        }
                        else
                        {
                            FullTableName ftn = FullTableName.FromTableNameContext(currentTSIJ.table_source_item().table_name_with_hint().table_name());
                            Console.WriteLine($"iterative: {ftn}");
                            leftSource = ftn.ToString();

                            if (selectContext.SourceTableName == null)
                                selectContext.SourceTableName = ftn;
                        }
                    }

                    if (currentTSIJ.join_part().Length > 0)
                    {
                        var joinContext = currentTSIJ.join_part()[0];
                        if (joinContext == null)
                            currentTSIJ = null;
                        else
                        {
                            // figure out which join type
                            if (joinContext.cross_join() != null)
                            {
                                // CROSS Join!

                                if (joinContext.cross_join().table_source().table_source_item_joined().table_source_item().derived_table() != null)
                                {
                                    SelectContext inner = GobbleSelectStatement(joinContext.cross_join().table_source().table_source_item_joined().table_source_item().derived_table().subquery()[0].select_statement());
                                    Console.WriteLine($"{leftSource} CROSS JOIN On subselect");

                                    JoinContext jc = new (JoinType.CROSS_JOIN, inner);
                                    PredicateContext pcon = new ();
                                    selectContext.AddJoin(jc, pcon);
                                }
                                else
                                {
                                    FullTableName otherTableName = FullTableName.FromTableNameContext(joinContext.cross_join().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name());
                                    Console.WriteLine($"{leftSource} CROSS JOIN On {otherTableName}");

                                    JoinContext jc = new (JoinType.CROSS_JOIN, otherTableName);
                                    PredicateContext pcon = new ();
                                    selectContext.AddJoin(jc, pcon);

                                }

                                currentTSIJ = joinContext.cross_join().table_source().table_source_item_joined();
                            }
                            else if (joinContext.join_on() != null)
                            {
                                // ON join

                                Expression x = GobbleSearchCondition(joinContext.join_on().search_condition());
                                PredicateContext pcon = new (x);

                                if (joinContext.join_on().table_source().table_source_item_joined().table_source_item().derived_table() != null)
                                {
                                    SelectContext inner = GobbleSelectStatement(joinContext.join_on().table_source().table_source_item_joined().table_source_item().derived_table().subquery()[0].select_statement());
                                    Console.WriteLine($"{leftSource} INNER JOIN On subselect");

                                    JoinContext jc = new (JoinType.INNER_JOIN, inner);
                                    selectContext.AddJoin(jc, pcon);
                                }
                                else
                                {
                                    FullTableName otherTableName = FullTableName.FromTableNameContext(joinContext.join_on().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name());
                                    Console.WriteLine($"{leftSource} INNER JOIN On {otherTableName}");

                                    JoinContext jc = new (JoinType.INNER_JOIN, otherTableName);
                                    selectContext.AddJoin(jc, pcon);
                                }


                                currentTSIJ = joinContext.join_on().table_source().table_source_item_joined();
                            }
                            else
                            {
                                throw new NotImplementedException("unsupported JOIN type enountered");
                            }

                        }
                    }
                    else
                        currentTSIJ = null;
                }

            }

            return selectContext;
        }
    }
}
