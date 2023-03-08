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
            SelectContext selectContext = GobbleQueryExpression(context.query_expression());

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

            return selectContext;
        }

        internal SelectContext GobbleQueryExpression(TSqlParser.Query_expressionContext context)
        {
            SelectContext selectContext = GobbleQuerySpecification(context.query_specification());

            // eat up unions
            foreach (var unionContext in context.sql_union())
            {
                var spec = unionContext.spec;
                var ex = unionContext.op;
                SelectContext? ctx = null;

                UnionType ut = UnionType.UNION;

                if (unionContext.ALL() != null)
                    ut = UnionType.UNION_ALL;
                else
                {
                    if (unionContext.INTERSECT() != null)
                        ut = UnionType.INTERSECT;
                    else if (unionContext.EXCEPT() != null)
                        ut = UnionType.EXCEPT;
                }

                if (spec != null)
                {
                    Console.WriteLine("Union: got spec");
                    ctx = GobbleQuerySpecification(spec);
                }

                if (ex != null)
                {
                    Console.WriteLine("Union: got ex");
                    ctx = GobbleQueryExpression(ex);
                }

                if (ctx == null)
                    throw new SemanticErrorException("expected unionContext");

                selectContext.AddUnionContext(ut, ctx);
            }

            return selectContext;
        }

        internal SelectContext GobbleQuerySpecification(TSqlParser.Query_specificationContext context)
        {
            PredicateContext? pc = null;

            // consume the WHERE predicate
            if (context.search_condition().Length > 0)
            {
                Expression x = GobbleSearchCondition(context.search_condition()[0]);
                pc = new PredicateContext(x);
            }

            SelectContext selectContext = new (context, pc);

            // get through the select list
            foreach (var elem in context.select_list().select_list_elem())
            {
                FullColumnName? fcn = null;
                Expression? x;

                if (elem.expression_elem() != null)
                {
                    // an expression, from a simple column name through a sub-select
                    x = GobbleSelectExpression(elem.expression_elem().expression(), selectContext);

                    // if we have an AS clause, then the name of this column comes from that
                    if (elem.expression_elem().as_column_alias() != null)
                    {
                        fcn = FullColumnName.FromIDContext(elem.expression_elem().as_column_alias().column_alias().id_());
                    }
                    else
                    {
                        // no AS clause; what if this expression has only a FromColumn?
                        fcn = x.GetExpressionColumnName();
                    }
                }
                else if (elem.asterisk() != null)
                {
                    Console.WriteLine("asterisk!");
                    continue;
                }
                else
                {
                    throw new InternalErrorException($"Unknown type in select list: {elem.GetText()}");
                }

                if (fcn != null)
                    selectContext.SelectListContext.AddRowsetColumnName(fcn);
                else
                    selectContext.SelectListContext.AddUnknownRowsetColumnName();

                Console.WriteLine($"SelectListElement:   {string.Join(" ", x)}");
                selectContext.AddSelectListExpressionList(x);
            }

            // the query_expression may have an order by,
            // and the query_expression might have one, too
            TSqlParser.Select_order_by_clauseContext? sobc = context.query_expression().select_order_by_clause();

            if (sobc != null && context.select_order_by_clause() != null)
                throw new InternalErrorException("Don't expect ORDER BY on both select_statement and query_expression");
            sobc ??= context.query_expression().select_order_by_clause();

            if (sobc != null)
            {
                OrderByContext obc = new ();

                foreach (var expr in sobc.order_by_clause().order_by_expression())
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
            foreach (var groupByItem in context.group_by_item())
            {
                Expression gbe = GobbleExpression(groupByItem.expression());
                selectContext.AddGroupByExpression(gbe);
            }

            // figure out sources
            if (context.table_sources() == null)
            {
                // no table source, so it's just from the "DUAL" table
            }
            else
            {
                // find the source table, along with all the joins
                var currentTSIJ = context.query_expression().query_specification().from.table_source()[0];

                string leftSource = "unassigned";
                if (currentTSIJ.table_source_item().derived_table() != null)
                {
                    // derived table ...
                    SelectContext inner = GobbleSelectStatement(currentTSIJ.table_source_item().derived_table().subquery()[0].select_statement());

                    if (currentTSIJ.table_source_item().as_table_alias() != null)
                        inner.DerivedTableAlias = ParseHelpers.StringFromIDContext(currentTSIJ.table_source_item().as_table_alias().table_alias().id_());
                    Console.WriteLine($"FROM: derived table AS {inner.DerivedTableAlias ?? "no alias"}");
                    leftSource = "Subselect";
                    selectContext.InputContext = inner;
                }
                else
                {
                    // FROM table ...
                    FullTableName ftn = FullTableName.FromFullTableNameContext(currentTSIJ.table_source_item().full_table_name());
                    FullTableName? ftnAlias = FullTableName.FromTableAliasContext(currentTSIJ.table_source_item().as_table_alias());
                    Console.WriteLine($"FROM: {ftn} AS {(ftnAlias == null ? "no alias" : ftnAlias)}");
                    leftSource = ftn.ToString();

                    selectContext.SourceTableName ??= ftn;

                    if (ftnAlias != null)
                        selectContext.DerivedTableAlias = ftnAlias.TableNameOnly;
                }


                while (currentTSIJ != null)
                {
                    TSqlParser.Table_sourceContext? nextTSIJ = null;

                    for (int jpi = 0; jpi < currentTSIJ.join_part().Length; jpi++)
                    {
                        var joinContext = currentTSIJ.join_part()[jpi];
                        if (joinContext == null)
                            nextTSIJ = null;
                        else
                        {
                            // figure out which join type
                            if (joinContext.cross_join() != null)
                            {
                                // CROSS Join!

                                if (joinContext.cross_join().table_source_item().derived_table() != null)
                                {
                                    // CROSS JOIN with a derived table ...
                                    SelectContext inner = GobbleSelectStatement(joinContext.cross_join().table_source_item().derived_table().subquery()[0].select_statement());

                                    JoinContext jc = new (JoinType.CROSS_JOIN, inner);
                                    PredicateContext pcon = new ();

                                    string? alias = null;
                                    if (joinContext.cross_join().table_source_item().as_table_alias() != null)
                                    {
                                        alias = ParseHelpers.StringFromIDContext(joinContext.cross_join().table_source_item().as_table_alias().table_alias().id_());
                                        jc.DerivedTableAlias = alias;
                                    }

                                    Console.WriteLine($"{leftSource} CROSS JOIN On sub-select {alias ?? "(no alias)"}");

                                    selectContext.AddJoin(jc, pcon);
                                }
                                else
                                {
                                    // CROSS JOIN with a table name ...
                                    FullTableName otherTableName = FullTableName.FromFullTableNameContext(joinContext.cross_join().table_source_item().full_table_name());

                                    JoinContext jc = new (JoinType.CROSS_JOIN, otherTableName);
                                    PredicateContext pcon = new ();

                                    string? alias = null;
                                    if (joinContext.cross_join().table_source_item().as_table_alias() != null)
                                    {
                                        alias = ParseHelpers.StringFromIDContext(joinContext.cross_join().table_source_item().as_table_alias().table_alias().id_());
                                        jc.DerivedTableAlias = alias;
                                    }

                                    Console.WriteLine($"{leftSource} CROSS JOIN On {otherTableName} {alias ?? "(no alias)"}");
                                    selectContext.AddJoin(jc, pcon);
                                }

                                nextTSIJ = joinContext.cross_join().table_source_item().table_source();
                            }
                            else if (joinContext.join_on() != null)
                            {
                                // ON join!  figure out the join type and direction

                                JoinType joinType = JoinType.INNER_JOIN;

                                if (joinContext.join_on().join_type != null)
                                {
                                    if (joinContext.join_on().join_type.Type == TSqlLexer.LEFT)
                                        joinType = JoinType.LEFT_OUTER_JOIN;
                                    else if (joinContext.join_on().join_type.Type == TSqlLexer.RIGHT)
                                        joinType = JoinType.RIGHT_OUTER_JOIN;
                                    else if (joinContext.join_on().join_type.Type == TSqlLexer.FULL)
                                        joinType = JoinType.FULL_OUTER_JOIN;
                                }

                                // get the ON predicate
                                Expression x = GobbleSearchCondition(joinContext.join_on().search_condition());
                                PredicateContext pcon = new (x);

                                // and work out the table sources ...
                                if (joinContext.join_on().table_source().table_source_item().derived_table() != null)
                                {
                                    // derived table
                                    SelectContext inner = GobbleSelectStatement(joinContext.join_on().table_source().table_source_item().derived_table().subquery()[0].select_statement());
                                    Console.WriteLine($"{leftSource} {joinType} On sub-select");

                                    string derivedTableAlias = ParseHelpers.StringFromIDContext(joinContext.join_on().table_source().table_source_item().as_table_alias().table_alias().id_());

                                    JoinContext jc = new (joinType, inner, derivedTableAlias);
                                    selectContext.AddJoin(jc, pcon);
                                }
                                else if (joinContext.join_on().table_source().table_source_item().full_table_name() != null)
                                {
                                    // plain old table_name_with_hint
                                    FullTableName otherTableName = FullTableName.FromFullTableNameContext(joinContext.join_on().table_source().table_source_item().full_table_name());
                                    Console.WriteLine($"{leftSource} {joinType} On {otherTableName}");

                                    if (joinContext.join_on().table_source().table_source_item().as_table_alias() != null)
                                    {
                                        string alias = ParseHelpers.StringFromIDContext(joinContext.join_on().table_source().table_source_item().as_table_alias().table_alias().id_());
                                        Console.WriteLine($"alias is {alias}");
                                    }

                                    JoinContext jc = new (joinType, otherTableName);
                                    selectContext.AddJoin(jc, pcon);
                                }
                                else
                                {
                                    throw new NotImplementedException("couldn't consume this JOIN table source");
                                }

                                nextTSIJ = joinContext.join_on().table_source();
                            }
                            else
                            {
                                throw new NotImplementedException("unsupported JOIN type encountered");
                            }

                        }
                    }

                    currentTSIJ = nextTSIJ;
                }
            }

            return selectContext;
        }
    }
}
