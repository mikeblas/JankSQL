
namespace JankSQL
{
    public class SelectContext : IExecutableContext
    {
        readonly TSqlParser.Select_statementContext statementContext;
        SelectListContext? selectList;

        // for WHERE clauses
        PredicateContext? predicateContext;

        readonly List<JoinContext> joinContexts = new ();
        readonly List<AggregateContext> aggregateContexts = new();
        readonly List<Expression> groupByExpressions = new();

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

        internal SelectContext(TSqlParser.Select_statementContext context)
        {
            statementContext = context;
        }

        internal PredicateContext? PredicateContext { get { return predicateContext; } set { predicateContext = value; } }


        internal void AddSelectListExpressionList(Expression expression)
        {
            if (selectList == null)
                throw new InternalErrorException("Expected a SelectList");

            selectList.AddSelectListExpressionList(expression);
        }

        internal SelectListContext SelectListContext { get { return selectList!; } set { selectList = value; } }

        public ExecuteResult Execute(Engines.IEngine engine)
        {
            if (selectList == null)
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
                {
                    throw new ExecutionException($"Table {sourceTableName} does not exist");
                }
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
                        {
                            throw new ExecutionException($"Joined table {j.OtherTableName} does not exist");
                        }

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

            // finally, see if we have an aggregation
            if (aggregateContexts.Count > 0)
            {
                Aggregation agger = new Aggregation(lastLeftOutput, aggregateContexts, groupByExpressions);
                lastLeftOutput = agger;
            }

            // then the select
            Select select = new Select(lastLeftOutput, querySpecs.select_list().select_list_elem(), selectList);
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
            if (selectList == null)
                Console.WriteLine("No select list found");
            else
                selectList.Dump();

            Console.WriteLine("PredicateExpressions:");
            if (predicateContext == null)
            {
                Console.WriteLine("  No predicate context");
            }
            else
            {
                for (int i = 0; i < predicateContext.PredicateExpressionListCount; i++)
                {
                    Console.Write($"  #{i}: ");
                    foreach (var x in predicateContext.PredicateExpressions[i])
                    {
                        Console.Write($"{x} ");
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
