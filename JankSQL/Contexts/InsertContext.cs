namespace JankSQL.Contexts
{
    using JankSQL.Operators;

    internal class InsertContext : IExecutableContext
    {
        private readonly TSqlParser.Insert_statementContext context;
        private List<FullColumnName>? targetColumns;
        private List<List<Expression>>? constructors = null;

        internal InsertContext(TSqlParser.Insert_statementContext context, FullTableName tableName)
        {
            this.context = context;
            this.TableName = tableName;
        }

        internal List<FullColumnName> TargetColumns
        {
            get { return targetColumns!; }
            set { targetColumns = value; }
        }

        internal FullTableName TableName { get; set; }

        public ExecuteResult Execute(Engines.IEngine engine)
        {
            if (constructors == null)
                throw new InternalErrorException("Expected a list of constructors");

            ExecuteResult results = new ExecuteResult();

            Engines.IEngineTable? engineTarget = engine.GetEngineTable(TableName);

            if (engineTarget == null)
                throw new ExecutionException($"Table {TableName} does not exist");
            else
            {
                // no target column names list means we implicitly use the list from the target table, in order
                if (targetColumns == null)
                {
                    targetColumns = new List<FullColumnName>();
                    for (int i = 0; i < engineTarget.ColumnCount; i++)
                        targetColumns.Add(engineTarget.ColumnName(i));
                }

                if (targetColumns.Count != constructors[0].Count)
                    throw new ExecutionException($"InsertContext expected {targetColumns.Count} columns, got {constructors[0].Count}");

                ConstantRowSource source = new (TargetColumns, constructors);
                Insert inserter = new (engineTarget, TargetColumns, source);

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

            if (TargetColumns == null)
                Console.WriteLine("   Columns: None found");
            else
            {
                string str = string.Join(',', TargetColumns);
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
                Console.WriteLine($"   Expressions: No constructors found");
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

                    Console.WriteLine(string.Join(',', expression.Select(x => $"[{x}]")));
                }
            }
        }

        internal void AddExpressionList(List<Expression> expressionList)
        {
            if (constructors is null)
                constructors = new ();

            this.constructors.Add(expressionList);
        }

        internal void AddExpressionLists(List<List<Expression>> expressionLists)
        {
            if (constructors is null)
                constructors = new ();

            this.constructors.AddRange(expressionLists);
        }
    }
}
