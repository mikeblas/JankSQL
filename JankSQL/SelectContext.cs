

namespace JankSQL
{
    public class SelectContext
    {
        TSqlParser.Select_statementContext statementContext;
        SelectListContext selectList;

        // for WHERE clauses
        List<List<ExpressionNode>> predicateExpressionLists = new List<List<ExpressionNode>>();
        List<ExpressionNode> currentExpressionList = new List<ExpressionNode>();

        List<JoinContext> joinContexts = new List<JoinContext>();


        internal void AddJoin(JoinContext jc)
        {
            joinContexts.Add(jc);
            jc.PredicateExpressions = predicateExpressionLists;
            predicateExpressionLists = new List<List<ExpressionNode>>();
        }

        internal SelectContext(TSqlParser.Select_statementContext context)
        {
            statementContext = context;
        }

        internal void EndPredicateExpressionList()
        {
            predicateExpressionLists.Add(currentExpressionList);
            currentExpressionList = new List<ExpressionNode>();
        }

        internal void EndAndCombinePredicateExpressionList(int arguments)
        {
            EndPredicateExpressionList();

            int firstIndex = predicateExpressionLists.Count - arguments -1;
            List<List<ExpressionNode>> range = predicateExpressionLists.GetRange(firstIndex, arguments+1);
            predicateExpressionLists.RemoveRange(firstIndex, arguments + 1);


            List<ExpressionNode> newList = new List<ExpressionNode>();
            foreach (var subList in range)
            {
                newList.AddRange(subList);
            }

            predicateExpressionLists.Add(newList);
        }

        internal void EndSelectListExpressionList()
        {
            selectList.AddSelectListExpressionList(currentExpressionList);
            currentExpressionList = new List<ExpressionNode>();
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
                if (sysTables.Row(i)[idxName].Equals(effectiveTableName, StringComparison.InvariantCultureIgnoreCase))
                {
                    foundRow = i;
                    break;
                }
            }
            if (foundRow == -1)
                return null;

            return sysTables.Row(foundRow)[idxFile];
        }

        internal ResultSet Execute()
        {
            ResultSet resultSet = null;

            var expressions = statementContext.query_expression();
            var querySpecs = expressions.query_specification();
            // var sourceTable = querySpecs.table_sources().table_source().First().GetText();
            string sourceTable = querySpecs.table_sources().table_source().First().table_source_item_joined().table_source_item().GetText();
            Console.WriteLine($"ExitSelect_Statement: {sourceTable}");

            string effectiveTableName = Program.GetEffectiveName(sourceTable);

            // get systables
            Engines.DynamicCSV sysTables = new Engines.DynamicCSV("sys_tables.csv", "sys_tables");
            sysTables.Load();

            // get the file name for our table
            string? effectiveTableFileName = FileFromSysTables(sysTables, effectiveTableName);

            if (effectiveTableFileName == null)
                Console.WriteLine($"Table {effectiveTableName} does not exist");
            else
            {
                // found the source table, so load it
                Engines.DynamicCSV table = new Engines.DynamicCSV(effectiveTableFileName, effectiveTableName);

                // the table itself
                TableSource tableSource = new TableSource(table);

                // any joins?
                IComponentOutput lastLeftOutput = tableSource;
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
                    Join oper = new Join();
                    oper.LeftInput = lastLeftOutput;
                    oper.RightInput = joinSource;
                    oper.PredicateExpressions = j.PredicateExpressions;

                    lastLeftOutput = oper;
                }

                // now the filter
                Filter filter = new Filter
                {
                    Input = lastLeftOutput,
                    Predicates = predicateExpressionLists
                };

                // then the select
                Select select = new Select(querySpecs.select_list().select_list_elem(), selectList);
                select.Input = filter;

                while (true)
                {
                    ResultSet batch = select.GetRows(5);
                    if (resultSet == null)
                        resultSet = ResultSet.NewWithShape(batch);
                    if (batch.RowCount == 0)
                        break;
                    resultSet.Append(batch);
                }
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
