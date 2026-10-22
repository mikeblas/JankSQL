namespace JankSQL.Contexts
{
    using JankSQL.Expressions;
    using JankSQL.Operators;

    internal class SelectContext : IExecutableContext
    {
        private readonly TSqlParser.Select_statementContext statementContext;

        private readonly List<JoinContext> joinContexts = new ();
        private readonly List<AggregateContext> aggregateContexts = new ();
        private readonly List<Expression> groupByExpressions = new ();
        private readonly SelectListContext selectListContext;

        // InvariantCultureIgnoreCase here so we can have localized table names
        private readonly HashSet<string> tableNames = new (StringComparer.InvariantCultureIgnoreCase);

        // for WHERE clauses
        private readonly PredicateContext? predicateContext;

        // once bound, we have our operator ready to go
        private Select? selectOperator = null;

        internal SelectContext(TSqlParser.Select_statementContext context, PredicateContext? predicateContext)
        {
            statementContext = context;
            this.predicateContext = predicateContext;
            selectListContext = new SelectListContext(context.query_expression().query_specification().select_list());

            SourceTableName = null;
            OrderByContext = null;
            InputContext = null;
        }

        internal OrderByContext? OrderByContext { get; set; }

        internal SelectListContext SelectListContext
        {
            get { return selectListContext!; }
        }

        internal FullTableName? SourceTableName { get; set; }

        internal string? DerivedTableAlias { get; set; }

        internal SelectContext? InputContext { get; set; }

        internal IOperatorOutput? ComputedSource { get; set; }

        public object Clone()
        {
            SelectContext clone = new(statementContext, predicateContext)
            {
                InputContext = InputContext != null ? (SelectContext)InputContext.Clone() : null,
                OrderByContext = OrderByContext != null ? (OrderByContext)OrderByContext.Clone() : null,
                SourceTableName = SourceTableName,
                DerivedTableAlias = DerivedTableAlias,
                ComputedSource = ComputedSource
            };

            clone.groupByExpressions.AddRange(groupByExpressions);
            foreach (var aggregate in aggregateContexts)
                clone.aggregateContexts.Add((AggregateContext)aggregate.Clone());

            foreach (var expr in selectListContext.SelectExpressions)
                clone.AddSelectListExpressionList(expr);

            for (int i = 0; i < selectListContext.RowsetColumnNameCount; i++)
                clone.SelectListContext.AddRowsetColumnName(selectListContext.RowsetColumnName(i));

            foreach (var jc in joinContexts)
                clone.joinContexts.Add((JoinContext)jc.Clone());

            return clone;
        }

        public BindResult Bind(Engines.IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            selectOperator = BuildSelectObject(engine);
            return selectOperator.Bind(engine, outerColumnNames, bindValues);
        }

        public ExecuteResult Execute(Engines.IEngine engine, IRowValueAccessor? outerAccessor, IDictionary<string, ExpressionOperand> bindValues)
        {
            ResultSet? resultSet = null;

            if (selectOperator == null)
                throw new InternalErrorException("Select operator is not bound");

            while (true)
            {
                ResultSet batch = selectOperator.GetRows(engine, outerAccessor, 5, bindValues);
                resultSet ??= ResultSet.NewWithShape(batch);

                if (batch.IsEOF)
                    break;

                resultSet.Append(batch);
            }

            ExecuteResult results = ExecuteResult.SuccessWithResultSet(resultSet);
            return results;
        }

        public void Dump()
        {
            Console.WriteLine("=====");
            Console.WriteLine("SELECT");
            if (selectListContext == null)
                Console.WriteLine("No select list found");
            else
                selectListContext.Dump();

            Console.WriteLine("PredicateExpressions:");
            if (predicateContext == null)
                Console.WriteLine("  No predicate context");
            else
            {
                //TODO: how to dump expressions with complex ExpressionNode objects (like IN operator)?
                for (int i = 0; i < predicateContext.PredicateExpressionListCount; i++)
                    Console.WriteLine($"  #{i}: {predicateContext.PredicateExpressions[i]}");
            }

            Console.WriteLine("Joins:");
            if (joinContexts.Count == 0)
                Console.WriteLine("  No join contexts");
            else
            {
                foreach (var join in joinContexts)
                    join.Dump();
            }

            Console.WriteLine("Aggregations:");
            if (aggregateContexts.Count == 0)
                Console.WriteLine("  No aggregations");
            else
            {
                foreach (var aggregate in aggregateContexts)
                    aggregate.Dump();
            }
        }

        internal void Reset()
        {
            tableNames.Clear();
        }

        internal Select BuildSelectObject(Engines.IEngine engine)
        {
            if (selectListContext == null)
                throw new InternalErrorException("Expected a SelectList");

            var expressions = statementContext.query_expression();
            var querySpecs = expressions.query_specification();

            // the top most output in this tree of objects
            IOperatorOutput lastLeftOutput;

            if (SourceTableName == null && InputContext == null)
            {
                // some computed source?
                if (ComputedSource != null)
                {
                    // but must be bound?
                    lastLeftOutput = ComputedSource;
                }
                else
                {
                    // no inputs -- it's just the "dual" source
                    lastLeftOutput = new TableSource(new Engines.DualSource());
                }
            }
            else
            {
                if (InputContext != null)
                {
                    lastLeftOutput = InputContext.BuildSelectObject(engine);
                }
                else
                {
                    if (SourceTableName == null)
                        throw new InternalErrorException("Expected valid SourceTableName");

                    Engines.IEngineTable? engineSource = engine.GetEngineTable(SourceTableName)
                        ?? throw new ExecutionException($"Table {SourceTableName} does not exist");

                    // found the source table, so hook it up
                    lastLeftOutput = new TableSource(engineSource, DerivedTableAlias);

                    AccumulateTableNames(SourceTableName, DerivedTableAlias);
                }

                // any joins?
                foreach (var j in joinContexts)
                {
                    IOperatorOutput joinSource;
                    if (j.SelectSource != null)
                    {
                        Select selectJoinSource = j.SelectSource.BuildSelectObject(engine);
                        if (j.DerivedTableAlias != null)
                            selectJoinSource.DerivedTableAlias = j.DerivedTableAlias;
                        joinSource = selectJoinSource;

                        AccumulateTableNames(j.SelectSource.tableNames, j.DerivedTableAlias);
                    }
                    else if (j.OtherTableName != null)
                    {
                        // find the other table
                        Engines.IEngineTable? otherTableSource = engine.GetEngineTable(j.OtherTableName)
                            ?? throw new ExecutionException($"Joined table {j.OtherTableName} does not exist");
                        joinSource = new TableSource(otherTableSource);

                        AccumulateTableNames(j.OtherTableName, j.DerivedTableAlias);
                    }
                    else
                    {
                        throw new InternalErrorException("incorrectly prepared Joincontext");
                    }

                    // build a join operator with it
                    Join oper = new (j.JoinType, lastLeftOutput, joinSource, j.PredicateExpressions, j.DerivedTableAlias);

                    lastLeftOutput = oper;
                }
            }

            // now the filter, if needed
            if (predicateContext?.PredicateExpressionListCount > 0)
            {
                Filter filter = new (lastLeftOutput, predicateContext.PredicateExpressions);
                lastLeftOutput = filter;
            }

            // next, see if we have an aggregation
            if (aggregateContexts.Count > 0)
            {
                // get names for all the expressions
                List<FullColumnName> groupByExpressionBindNames = new ();
                foreach (var gbe in groupByExpressions)
                {
                    FullColumnName? bindName = selectListContext.BindNameForExpression(gbe);
                    if (bindName != null)
                        groupByExpressionBindNames.Add(bindName);
                }

                // make sure there are no uncovered non-aggregate functions
                foreach (var expr in selectListContext.SelectExpressions)
                {
                    // if it has an aggregate function, it's fine
                    if (expr.ContainsAggregate)
                        continue;

                    // otherwise, it needs a match in the group expressions
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

                Aggregation agger = new (lastLeftOutput, aggregateContexts, groupByExpressions, groupByExpressionBindNames);
                lastLeftOutput = agger;
            }

            // and check for an order by
            if (OrderByContext != null)
            {
                Sort sort = new (lastLeftOutput, OrderByContext.ExpressionList, OrderByContext.IsAscendingList);

                lastLeftOutput = sort;
            }

            // then the select
            Select select = new (lastLeftOutput, querySpecs.select_list().select_list_elem(), selectListContext, null /* derivedTableAlias */);
            return select;
        }

        internal void AddJoin(JoinContext jc, PredicateContext predicateContext)
        {
            joinContexts.Add(jc);
            if (predicateContext != null)
                jc.PredicateExpressions = predicateContext.PredicateExpressions;
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

        internal void AddSelectListExpressionList(Expression expression)
        {
            if (selectListContext == null)
                throw new InternalErrorException("Expected a SelectList");

            selectListContext.AddSelectListExpressionList(expression);
        }

        internal void AccumulateTableNames(FullTableName ftn, string? aliasName)
        {
            string effectiveName = aliasName ?? ftn.TableNameOnly;

            AccumulateTableName(effectiveName);
        }

        internal void AccumulateTableNames(IEnumerable<string> others, string? aliasName)
        {
            if (aliasName != null)
                AccumulateTableName(aliasName);
            else
            {
                foreach (var other in others)
                    AccumulateTableName(other);
            }
        }

        internal void AccumulateTableName(string name)
        {
            if (tableNames.Contains(name))
                throw new ExecutionException($"object name {name} is ambiguous");
            tableNames.Add(name);
        }
    }
}
