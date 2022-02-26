

namespace JankSQL
{
    public class SelectContext
    {
        TSqlParser.Select_statementContext statementContext;
        SelectListContext selectList;

        // for WHERE clauses
        List<Expression> predicateExpressionLists = new List<Expression>();
        Expression currentExpressionList = new Expression();

        List<JoinContext> joinContexts = new List<JoinContext>();


        internal void AddJoin(JoinContext jc)
        {
            joinContexts.Add(jc);
            jc.PredicateExpressions = predicateExpressionLists;
            predicateExpressionLists = new List<Expression>();
        }

        internal SelectContext(TSqlParser.Select_statementContext context)
        {
            statementContext = context;
        }

        internal void EndPredicateExpressionList()
        {
            predicateExpressionLists.Add(currentExpressionList);
            currentExpressionList = new Expression();
        }

        internal void EndAndCombinePredicateExpressionList(int arguments)
        {
            EndPredicateExpressionList();

            int firstIndex = predicateExpressionLists.Count - arguments -1;
            List<Expression> range = predicateExpressionLists.GetRange(firstIndex, arguments+1);
            predicateExpressionLists.RemoveRange(firstIndex, arguments + 1);


            Expression newList = new Expression();
            foreach (var subList in range)
            {
                newList.AddRange(subList);
            }

            predicateExpressionLists.Add(newList);
        }

        internal void EndSelectListExpressionList()
        {
            selectList.AddSelectListExpressionList(currentExpressionList);
            currentExpressionList = new Expression();
        }

        internal List<ExpressionNode> ExpressionList { get { return currentExpressionList; } }

        internal int PredicateExpressionListCount { get { return predicateExpressionLists.Count; } }

        internal SelectListContext SelectListContext { get { return selectList; } set { selectList = value; } }

        string? FileFromSysTables(Engines.DynamicCSV sysTables, string effectiveTableName)
        {
            // is this source table in there?
            int idxName = sysTables.ColumnIndex("table_name");
            int idxFile = sysTables.ColumnIndex("file_name");

            int foundRow = -1;
            for (int i = 0; i < sysTables.RowCount; i++)
            {
                if (sysTables.Row(i)[idxName].AsString().Equals(effectiveTableName, StringComparison.InvariantCultureIgnoreCase))
                {
                    foundRow = i;
                    break;
                }
            }
            if (foundRow == -1)
                return null;

            return sysTables.Row(foundRow)[idxFile].AsString();
        }

        internal ResultSet Execute()
        {
            ResultSet resultSet = null;

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
                string? effectiveTableFileName = FileFromSysTables(sysTables, effectiveTableName);

                if (effectiveTableFileName == null)
                {
                    Console.WriteLine($"Table {effectiveTableName} does not exist");
                    throw new InvalidOperationException();
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
                        string? otherTableFileName = FileFromSysTables(sysTables, otherEffectiveTableName);
                        if (otherTableFileName == null)
                        {
                            Console.WriteLine($"Joined table {j.OtherTableName} does not exist");
                            return null;
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
            if (predicateExpressionLists.Count() > 0)
            {
                Filter filter = new Filter(lastLeftOutput, predicateExpressionLists);
                lastLeftOutput = filter;
            }

            // then the select
            Select select = new Select(lastLeftOutput, querySpecs.select_list().select_list_elem(), selectList);

            while (true)
            {
                ResultSet batch = select.GetRows(5);
                if (resultSet == null)
                    resultSet = ResultSet.NewWithShape(batch);
                if (batch.RowCount == 0)
                    break;
                resultSet.Append(batch);
            }

            return resultSet;
        }


        internal void Dump()
        {
            selectList.Dump();

            Console.WriteLine("PredicateExpressions:");
            for (int i = 0; i < PredicateExpressionListCount; i++)
            {
                Console.Write($"  #{i}: ");
                foreach (var x in predicateExpressionLists[i])
                {
                    Console.Write($"{x} ");
                }
                Console.WriteLine();
            }
        }
    }
}
