
namespace JankSQL
{
    public class SelectContext : IExecutableContext
    {
        TSqlParser.Select_statementContext statementContext;
        SelectListContext? selectList;

        // for WHERE clauses
        PredicateContext predicateContext;

        List<JoinContext> joinContexts = new List<JoinContext>();


        internal void AddJoin(JoinContext jc, PredicateContext predicateContext)
        {
            joinContexts.Add(jc);
            if (predicateContext != null)
                jc.PredicateExpressions = predicateContext.PredicateExpressions;
            predicateContext = new PredicateContext();
        }

        internal SelectContext(TSqlParser.Select_statementContext context)
        {
            statementContext = context;
        }

        internal PredicateContext PredicateContext { get { return predicateContext; } set { predicateContext = value; } }


        internal void EndSelectListExpressionList(Expression expression)
        {
            if (selectList == null)
                throw new InternalErrorException("Expected a SelectList");

            selectList.AddSelectListExpressionList(expression);
        }

        internal SelectListContext? SelectListContext { get { return selectList; } set { selectList = value; } }

        public ExecuteResult Execute()
        {
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
                string sourceTable = querySpecs.table_sources().table_source().First().table_source_item_joined().table_source_item().GetText();
                Console.WriteLine($"ExitSelect_Statement: {sourceTable}");

                string effectiveTableName = Program.GetEffectiveName(sourceTable);

                // get systables
                Engines.DynamicCSV sysTables = new Engines.DynamicCSV("sys_tables.csv", "sys_tables");
                sysTables.Load();

                // get the file name for our table
                string? effectiveTableFileName = Engines.DynamicCSV.FileFromSysTables(sysTables, effectiveTableName);

                if (effectiveTableFileName == null)
                {
                    throw new ExecutionException($"Table {effectiveTableName} does not exist");
                }
                else
                {
                    // found the source table, so load it
                    Engines.DynamicCSV table = new Engines.DynamicCSV(effectiveTableFileName, effectiveTableName);

                    // hook it up
                    tableSource = new TableSource(table);
                    lastLeftOutput = tableSource;

                    // any joins?
                    foreach (var j in joinContexts)
                    {
                        // find the other table
                        string otherEffectiveTableName = Program.GetEffectiveName(j.OtherTableName);
                        string? otherTableFileName = Engines.DynamicCSV.FileFromSysTables(sysTables, otherEffectiveTableName);
                        if (otherTableFileName == null)
                        {
                            throw new ExecutionException($"Joined table {j.OtherTableName} does not exist");
                        }

                        // get a table engine on it
                        Engines.DynamicCSV otherTable = new Engines.DynamicCSV(otherTableFileName, otherEffectiveTableName);
                        TableSource joinSource = new TableSource(otherTable);

                        // build a join operator with it
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
