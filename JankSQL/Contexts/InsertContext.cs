namespace JankSQL.Contexts
{
    using JankSQL.Expressions;
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

        internal IList<FullColumnName> TargetColumns
        {
            get { return targetColumns!; }
            set { targetColumns = value.ToList(); }
        }

        internal FullTableName TableName { get; set; }

        public object Clone()
        {
            InsertContext clone = new (context, TableName);

            if (targetColumns != null)
                clone.TargetColumns = TargetColumns;

            if (constructors != null)
                clone.AddExpressionLists(constructors);

            return clone;
        }

        public BindResult Bind(Engines.IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            Console.WriteLine("WARNING: Bind() not implemented for InsertContext");
            return new(BindStatus.SUCCESSFUL);
        }


        public ExecuteResult Execute(Engines.IEngine engine, IRowValueAccessor? accessor, IDictionary<string, ExpressionOperand> bindValues)
        {
            if (constructors == null)
                throw new InternalErrorException("Expected a list of constructors");

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

                while (true)
                {
                    ResultSet batch = inserter.GetRows(engine, accessor, 5, bindValues);
                    if (batch.IsEOF)
                        break;
                }

                ExecuteResult result = ExecuteResult.SuccessWithRowsAffected(inserter.RowsAffected);
                return result;
            }
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
            name = String.Join(',',
                constructors.Select(x => "{" +
                    String.Join(',',
                            x.Select(y => "[" + y + "]"))
                        + "}"));
            Console.WriteLine($"   Expressions: {name}");
            */

            if (constructors == null || constructors.Count == 0)
                Console.WriteLine("   Expressions: No constructors found");
            else
            {
                bool first = true;
                foreach (var expression in constructors)
                {
                    if (first)
                    {
                        Console.Write("   Expressions: ");
                        first = false;
                    }
                    else
                        Console.Write("                ");

                    Console.Write($"len={expression.Count} ");

                    Console.WriteLine(string.Join(',', expression.Select(x => $"[{x}]")));
                }
            }
        }

        internal void AddExpressionList(List<Expression> expressionList)
        {
#pragma warning disable IDE0074 // Use compound assignment
            if (constructors is null)
                constructors = [];
#pragma warning restore IDE0074 // Use compound assignment

            this.constructors.Add(expressionList);
        }

        internal void AddExpressionLists(List<List<Expression>> expressionLists)
        {
#pragma warning disable IDE0074 // Use compound assignment
            if (constructors is null)
                constructors = [];
#pragma warning restore IDE0074 // Use compound assignment

            this.constructors.AddRange(expressionLists);
        }
    }
}
