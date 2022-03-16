
namespace JankSQL
{
    internal class InsertContext : IExecutableContext
    {
        TSqlParser.Insert_statementContext context;
        List<FullColumnName>? targetColumns;

        internal InsertContext(TSqlParser.Insert_statementContext context, FullTableName tableName)
        {
            this.context = context;
            this.TableName = tableName;
        }

        internal List<FullColumnName> TargetColumns { get { return targetColumns!; } set { targetColumns = value; } }

        internal List<List<Expression>>? constructors = null;
        
        internal FullTableName TableName { get; set; }

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

        public ExecuteResult Execute(Engines.IEngine engine)
        {
            if (constructors == null)
                throw new InternalErrorException("Expected a list of constructors");

            ExecuteResult results = new ExecuteResult();

            Engines.IEngineTable? engineTarget = engine.GetEngineTable(TableName);

            if (engineTarget == null)
            {
                throw new ExecutionException($"Table {TableName} does not exist");
            }
            else
            {
                if (engineTarget.ColumnCount != constructors[0].Count)
                {
                    throw new ExecutionException($"InsertContext expected {engineTarget.ColumnCount} columns, got {constructors[0].Count}");
                }

                ConstantRowSource source = new ConstantRowSource(TargetColumns, constructors);
                Insert inserter = new Insert(engineTarget, source);

                ResultSet? resultSet = null;

                while (true)
                {
                    ResultSet? batch = inserter.GetRows(5);
                    if (batch == null)
                        break;
                    if (resultSet == null)
                        resultSet = ResultSet.NewWithShape(batch);
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

            if (constructors == null || constructors.Count == 0)
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
