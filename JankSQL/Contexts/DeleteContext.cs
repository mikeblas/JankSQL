namespace JankSQL.Contexts
{
    using JankSQL.Operators;

    internal class DeleteContext : IExecutableContext
    {
        private readonly FullTableName tableName;
        private PredicateContext? predicateContext;

        internal DeleteContext(FullTableName tableName)
        {
            this.tableName = tableName;
        }

        internal PredicateContext PredicateContext
        {
            get { return predicateContext!; } set { predicateContext = value; }
        }

        public void Dump()
        {
            Console.WriteLine($"DELETE FROM {tableName}");

            if (predicateContext == null || predicateContext.PredicateExpressionListCount == 0)
            {
                Console.WriteLine("   no predicates");
            }
            else
            {
                for (int i = 0; i < predicateContext.PredicateExpressionListCount; i++)
                {
                    Console.WriteLine($"    {predicateContext.PredicateExpressions[i]}");
                }
            }
        }

        public ExecuteResult Execute(Engines.IEngine engine)
        {
            ExecuteResult results = new ExecuteResult();

            Engines.IEngineTable? tableSource = engine.GetEngineTable(tableName);

            if (tableSource == null)
            {
                throw new ExecutionException($"Table {tableName} does not exist");
            }
            else
            {
                // found the source table, so load it
                TableSource source = new (tableSource);
                Delete delete = new Delete(tableSource, source, PredicateContext.PredicateExpressions);

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
