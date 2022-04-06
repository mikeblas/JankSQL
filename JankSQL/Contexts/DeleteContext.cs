namespace JankSQL.Contexts
{
    using JankSQL.Operators;

    internal class DeleteContext : IExecutableContext
    {
        private readonly FullTableName tableName;
        private Expression? predicateExpression;

        internal DeleteContext(FullTableName tableName)
        {
            this.tableName = tableName;
        }

        internal Expression? PredicateExpression
        {
            get { return predicateExpression; }
            set { predicateExpression = value; }
        }

        public void Dump()
        {
            Console.WriteLine($"DELETE FROM {tableName}");

            if (predicateExpression == null)
                Console.WriteLine("   no predicate");
            else
                Console.WriteLine($"       {predicateExpression}");
        }

        public ExecuteResult Execute(Engines.IEngine engine)
        {
            ExecuteResult results = new ();

            Engines.IEngineTable? tableSource = engine.GetEngineTable(tableName);

            if (tableSource == null)
            {
                throw new ExecutionException($"Table {tableName} does not exist");
            }
            else
            {
                // found the source table, so load it
                TableSource source = new (tableSource);
                Delete delete = new (tableSource, source, predicateExpression);

                while (true)
                {
                    ResultSet? batch = delete.GetRows(5);
                    if (batch == null)
                        break;
                }

                results.ExecuteStatus = ExecuteStatus.SUCCESSFUL;
            }

            return results;
        }
    }
}
