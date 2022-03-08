
namespace JankSQL
{
    internal class InsertContext : IExecutableContext
    {
        TSqlParser.Insert_statementContext context;
        List<FullColumnName>? targetColumns;

        internal InsertContext(TSqlParser.Insert_statementContext context)
        {
            this.context = context;
        }

        internal List<FullColumnName>? TargetColumns { get { return targetColumns; } set { targetColumns = value; } }

        internal List<List<Expression>>? constructors = null;
        
        internal string TableName { get; set; }

        internal void AddExpressionList(List<Expression> expressionList)
        {
            if (constructors is null)
                constructors = new();

            this.constructors.Add(expressionList);
        }

        internal void AddExpressionLists(List<List<Expression>> expressionLists)
        {
            if (constructors is null)
                constructors = new();

            this.constructors.AddRange(expressionLists);
        }

        public ExecuteResult Execute()
        {
            if (constructors == null)
                throw new InternalErrorException("Expected a list of constructors");

            ExecuteResult results = new ExecuteResult();

            string effectiveTableName = Program.GetEffectiveName(TableName);

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
                // we've got our table ...
                Engines.DynamicCSV table = new Engines.DynamicCSV(effectiveTableFileName, effectiveTableName);
                table.Load();

                if (table.ColumnCount != constructors[0].Count)
                {
                    throw new ExecutionException($"Expected {table.ColumnCount} columns, got {constructors[0].Count}");
                }

                ConstantRowSource source = new ConstantRowSource(targetColumns, constructors);
                Insert inserter = new Insert(table, source);

                ResultSet? resultSet = null;

                while (true)
                {
                    ResultSet? batch = inserter.GetRows(5);
                    if (resultSet == null)
                        resultSet = ResultSet.NewWithShape(batch);
                    if (batch == null)
                        break;
                    resultSet.Append(batch);
                }

                results.ResultSet = resultSet;
                results.ExecuteStatus = ExecuteStatus.SUCCESSFUL;
            }

            return results;
        }

        public void Dump()
        {
            Console.WriteLine($"INSERT into {TableName}");

            string str;

            if (TargetColumns == null)
            {
                Console.WriteLine("   Columns: None found");
            }
            else
            {
                str = string.Join(',', TargetColumns);
                Console.WriteLine($"   Columns: {str}");
            }

            /*
            str = String.Join(',',
                constructors.Select(x => "{" +
                    String.Join(',',
                            x.Select(y => "[" + y + "]"))
                        + "}"));
            Console.WriteLine($"   Expressions: {str}");
            */

            if (constructors == null)
            {
                Console.WriteLine($"   Expressions: No constructors found");
            }
            else
            {
                bool first = true;
                foreach (var expression in constructors)
                {
                    if (first)
                    {
                        Console.Write($"   Expressions: ");
                        first = false;
                    }
                    else
                        Console.Write($"                ");

                    Console.Write($"len={expression.Count} ");

                    Console.WriteLine(String.Join(',', expression.Select(x => "[" + x + "]")));
                }
            }

        }
    }
}
