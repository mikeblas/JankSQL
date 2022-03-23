namespace JankSQL.Contexts
{
    using JankSQL.Operators;

    public class SelectContext : IExecutableContext
    {
        private readonly TSqlParser.Select_statementContext statementContext;

        private readonly List<JoinContext> joinContexts = new ();
        private readonly List<AggregateContext> aggregateContexts = new ();
        private readonly List<Expression> groupByExpressions = new ();

        private OrderByContext? orderByContext;

        private SelectListContext? selectListContext;

        // for WHERE clauses
        private PredicateContext? predicateContext;

        internal SelectContext(TSqlParser.Select_statementContext context)
        {
            statementContext = context;
        }

        public ExecuteResult Execute(Engines.IEngine engine)
        {
            if (selectListContext == null)
                throw new InternalErrorException("Expected a SelectList");

            ExecuteResult results = new ExecuteResult();

            var expressions = statementContext.query_expression();
            var querySpecs = expressions.query_specification();

            // the table itself
            TableSource tableSource;
            IComponentOutput lastLeftOutput;

            if (querySpecs.table_sources() == null)
            {
                // no source table!
                tableSource = new TableSource(new Engines.DualSource());
                lastLeftOutput = tableSource;
            }
            else
            {
                FullTableName sourceTableName = FullTableName.FromTableNameContext(querySpecs.table_sources().table_source().First().table_source_item_joined().table_source_item().table_name_with_hint().table_name());
                Console.WriteLine($"ExitSelect_Statement: {sourceTableName}");

                Engines.IEngineTable? engineSource = engine.GetEngineTable(sourceTableName);
                if (engineSource == null)
                    throw new ExecutionException($"Table {sourceTableName} does not exist");
                else
                {
                    // found the source table, so hook it up
                    tableSource = new TableSource(engineSource);
                    lastLeftOutput = tableSource;

                    // any joins?
                    foreach (var j in joinContexts)
                    {
                        // find the other table
                        Engines.IEngineTable? otherTableSource = engine.GetEngineTable(j.OtherTableName);
                        if (otherTableSource == null)
                            throw new ExecutionException($"Joined table {j.OtherTableName} does not exist");

                        // build a join operator with it
                        TableSource joinSource = new TableSource(otherTableSource);
                        Join oper = new Join(j.JoinType, lastLeftOutput, joinSource, j.PredicateExpressions);

                        lastLeftOutput = oper;
                    }
                }
            }

            // now the filter, if needed
            if (predicateContext != null && predicateContext.PredicateExpressionListCount > 0)
            {
                Filter filter = new Filter(lastLeftOutput, predicateContext.PredicateExpressions);
                lastLeftOutput = filter;
            }

            // next, see if we have an aggregation
            if (aggregateContexts.Count > 0)
            {
                // get names for all the expressions
                List<string> groupByExpressionBindNames = new();
                foreach (var gbe in groupByExpressions)
                {
                    string? bindName = selectListContext.BindNameForExpression(gbe);
                    if (bindName != null)
                        groupByExpressionBindNames.Add(bindName);
                }

                // make sure there are no uncovered non-aggregate functions
                foreach (var expr in selectListContext.SelectExpressions)
                {
                    // if it has an aggregate function, it's fine
                    if (expr.ContainsAggregate)
                        continue;

                    // otherwise, it needs a match in the group expressoins
                    bool found = false;
                    foreach (var gbe in groupByExpressions)
                    {
                        if (gbe.Equals(expr))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        throw new ExecutionException($"non-aggregate {expr} in select list is not covered in GROUP BY");
                }

                Aggregation agger = new Aggregation(lastLeftOutput, aggregateContexts, groupByExpressions, groupByExpressionBindNames);
                lastLeftOutput = agger;
            }

            // and check for an order by
            if (orderByContext != null)
            {
                Sort sort = new Sort(lastLeftOutput, orderByContext.ExpressionList, orderByContext.IsAscendingList);

                lastLeftOutput = sort;
            }

            // then the select
            Select select = new Select(lastLeftOutput, querySpecs.select_list().select_list_elem(), selectListContext);
            ResultSet? resultSet = null;

            while (true)
            {
                ResultSet? batch = select.GetRows(5);
                if (batch == null)
                    break;
                if (resultSet == null)
                    resultSet = ResultSet.NewWithShape(batch);
                resultSet.Append(batch);
            }

            results.ResultSet = resultSet;
            return results;
        }

        public void Dump()
        {
            if (selectListContext == null)
                Console.WriteLine("No select list found");
            else
                selectListContext.Dump();

            Console.WriteLine("PredicateExpressions:");
            if (predicateContext == null)
                Console.WriteLine("  No predicate context");
            else
            {
                for (int i = 0; i < predicateContext.PredicateExpressionListCount; i++)
                {
                    Console.Write($"  #{i}: ");
                    foreach (var x in predicateContext.PredicateExpressions[i])
                        Console.Write($"{x} ");

                    Console.WriteLine();
                }
            }
        }

        internal void AddJoin(JoinContext jc, PredicateContext predicateContext)
        {
            joinContexts.Add(jc);
            if (predicateContext != null)
                jc.PredicateExpressions = predicateContext.PredicateExpressions;
            predicateContext = new PredicateContext();
        }

        internal void AddAggregate(AggregateContext ac)
        {
            ac.ExpressionName = $"Aggregate{aggregateContexts.Count + 1001}";
            aggregateContexts.Add(ac);
        }

        internal void AddGroupByExpression(Expression expression)
        {
            groupByExpressions.Add(expression);
        }

        internal PredicateContext? PredicateContext
        {
            get { return predicateContext; } set { predicateContext = value; }
        }

        internal OrderByContext? OrderByContext
        {
            get { return orderByContext; } set { orderByContext = value; }
        }

        internal SelectListContext SelectListContext
        {
            get { return selectListContext!; }
            set { selectListContext = value; }
        }

        internal void AddSelectListExpressionList(Expression expression)
        {
            if (selectListContext == null)
                throw new InternalErrorException("Expected a SelectList");

            selectListContext.AddSelectListExpressionList(expression);
        }

    }
}
