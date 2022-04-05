﻿namespace JankSQL.Contexts
{
    using JankSQL.Operators;

    public class SelectContext : IExecutableContext
    {
        private readonly TSqlParser.Select_statementContext statementContext;

        private readonly List<JoinContext> joinContexts = new ();
        private readonly List<AggregateContext> aggregateContexts = new ();
        private readonly List<Expression> groupByExpressions = new ();
        private readonly SelectListContext selectListContext;

        // for WHERE clauses
        private readonly PredicateContext? predicateContext;

        private FullTableName? sourceTableName;

        private SelectContext? inputContext;

        private OrderByContext? orderByContext;

        private string? derivedTableAlias;

        internal SelectContext(TSqlParser.Select_statementContext context, PredicateContext? predicateContext)
        {
            statementContext = context;
            this.predicateContext = predicateContext;
            selectListContext = new SelectListContext(context.query_expression().query_specification().select_list());

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

        internal Select BuildSelectObject(Engines.IEngine engine)
        {
            if (selectListContext == null)
                throw new InternalErrorException("Expected a SelectList");

            var expressions = statementContext.query_expression();
            var querySpecs = expressions.query_specification();

            // the table itself
            IComponentOutput lastLeftOutput;

            if (SourceTableName == null && inputContext == null)
            {
                // no inputs -- it's just the "dual" source
                lastLeftOutput = new TableSource(new Engines.DualSource());
            }
            else
            {
                Console.WriteLine($"ExitSelect_Statement: {SourceTableName}");

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
                }

                // any joins?
                foreach (var j in joinContexts)
                {
                    IComponentOutput joinSource;
                    if (j.SelectSource != null)
                    {
                        joinSource = j.SelectSource.BuildSelectObject(engine);
                    }
                    else if (j.OtherTableName != null)
                    {
                        // find the other table
                        Engines.IEngineTable? otherTableSource = engine.GetEngineTable(j.OtherTableName);
                        if (otherTableSource == null)
                            throw new ExecutionException($"Joined table {j.OtherTableName} does not exist");

                        joinSource = new TableSource(otherTableSource);
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
            Select select = new Select(lastLeftOutput, querySpecs.select_list().select_list_elem(), selectListContext, derivedTableAlias);
            return select;
        }

        public ExecuteResult Execute(Engines.IEngine engine)
        {
            Select select = BuildSelectObject(engine);
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

            ExecuteResult results = new ExecuteResult();
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

        internal void AddSelectListExpressionList(Expression expression)
        {
            if (selectListContext == null)
                throw new InternalErrorException("Expected a SelectList");

            selectListContext.AddSelectListExpressionList(expression);
        }

    }
}
