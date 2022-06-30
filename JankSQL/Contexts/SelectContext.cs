﻿namespace JankSQL.Contexts
{
    using JankSQL.Expressions;
    using JankSQL.Operators;

    internal class SelectContext : IExecutableContext
    {
        private readonly TSqlParser.Query_specificationContext querySpecification;

        private readonly List<JoinContext> joinContexts = new ();
        private readonly List<AggregateContext> aggregateContexts = new ();
        private readonly List<Expression> groupByExpressions = new ();
        private readonly SelectListContext selectListContext;
        private readonly HashSet<string> tableNames = new (StringComparer.OrdinalIgnoreCase);
        private readonly List<(UnionType ut, SelectContext selectContext)> unionContexts = new ();

        // for WHERE clauses
        private readonly PredicateContext? predicateContext;

        private FullTableName? sourceTableName;

        private SelectContext? inputContext;

        private OrderByContext? orderByContext;

        private string? derivedTableAlias;

        internal SelectContext(TSqlParser.Query_specificationContext context, PredicateContext? predicateContext)
        {
            querySpecification = context;
            this.predicateContext = predicateContext;
            selectListContext = new SelectListContext(querySpecification.select_list());

            sourceTableName = null;
            orderByContext = null;
            inputContext = null;
        }

        internal OrderByContext? OrderByContext
        {
            get { return orderByContext; }
            set { orderByContext = value; }
        }

        internal SelectListContext SelectListContext
        {
            get { return selectListContext!; }
        }

        internal FullTableName? SourceTableName
        {
            get { return sourceTableName; }
            set { sourceTableName = value; }
        }

        internal string? DerivedTableAlias
        {
            get { return derivedTableAlias; }
            set { derivedTableAlias = value; }
        }

        internal SelectContext? InputContext
        {
            get { return inputContext; }
            set { inputContext = value; }
        }


        public object Clone()
        {
            SelectContext clone = new (querySpecification, predicateContext);

            clone.inputContext = inputContext != null ? (SelectContext)inputContext.Clone() : null;
            clone.orderByContext = orderByContext != null ? (OrderByContext)orderByContext.Clone() : null;
            clone.sourceTableName = sourceTableName;
            clone.derivedTableAlias = derivedTableAlias;

            foreach (var groupBy in groupByExpressions)
                clone.groupByExpressions.Add(groupBy);
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

        public ExecuteResult Execute(Engines.IEngine engine, IRowValueAccessor? outerAccessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            Select select = BuildSelectObject(engine);
            ResultSet? resultSet = null;

            while (true)
            {
                ResultSet batch = select.GetRows(engine, outerAccessor, 5, bindValues);
                if (resultSet == null)
                    resultSet = ResultSet.NewWithShape(batch);

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

            foreach (var unionContext in unionContexts)
            {
                Console.WriteLine($"  Got union context: {unionContext.ut}");
                unionContext.selectContext.Dump();
            }
        }

        internal void Reset()
        {
            tableNames.Clear();
        }

        internal void AddUnionContext(UnionType ut, SelectContext unionContext)
        {
            unionContexts.Add((ut, unionContext));
        }

        internal Select BuildSelectObject(Engines.IEngine engine)
        {
            if (selectListContext == null)
                throw new InternalErrorException("Expected a SelectList");

//            var querySpecs = expressionContext.query_specification();

            // the top most output in this tree of objects
            IComponentOutput lastLeftOutput;

            if (SourceTableName == null && inputContext == null)
            {
                // no inputs -- it's just the "dual" source
                lastLeftOutput = new TableSource(new Engines.DualSource());
            }
            else
            {
                if (inputContext != null)
                {
                    lastLeftOutput = inputContext.BuildSelectObject(engine);
                }
                else
                {
                    if (SourceTableName == null)
                        throw new InternalErrorException("Expected valid SourceTableName");

                    Engines.IEngineTable? engineSource = engine.GetEngineTable(SourceTableName);
                    if (engineSource == null)
                        throw new ExecutionException($"Table {SourceTableName} does not exist");

                    // found the source table, so hook it up
                    lastLeftOutput = new TableSource(engineSource, DerivedTableAlias);

                    AccumulateTableNames(SourceTableName, DerivedTableAlias);
                }

                // any joins?
                foreach (var j in joinContexts)
                {
                    IComponentOutput joinSource;
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
                        Engines.IEngineTable? otherTableSource = engine.GetEngineTable(j.OtherTableName);
                        if (otherTableSource == null)
                            throw new ExecutionException($"Joined table {j.OtherTableName} does not exist");

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
            if (predicateContext != null && predicateContext.PredicateExpressionListCount > 0)
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

                Aggregation agger = new (lastLeftOutput, aggregateContexts, groupByExpressions, groupByExpressionBindNames);
                lastLeftOutput = agger;
            }

            // and check for an order by
            if (orderByContext != null)
            {
                Sort sort = new (lastLeftOutput, orderByContext.ExpressionList, orderByContext.IsAscendingList);

                lastLeftOutput = sort;
            }

            // then the select
            Select select = new (lastLeftOutput, querySpecification.select_list().select_list_elem(), selectListContext, null /* derivedTableAlias */);
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


