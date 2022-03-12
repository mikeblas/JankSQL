
namespace JankSQL
{
    internal class DeleteContext : IExecutableContext
    {
        FullTableName tableName;
        PredicateContext? predicateContext;

        internal DeleteContext(FullTableName tableName)
        {
            this.tableName = tableName;
        }

        internal PredicateContext PredicateContext { get { return predicateContext!; } set { predicateContext = value; } }

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

            Engines.IEngineSource? tableSource = engine.GetSourceTable(tableName);
            Engines.IEngineDestination? tableDestination = engine.GetDestinationTable(tableName);

            if (tableSource == null || tableDestination == null)
            {
                throw new ExecutionException($"Table {tableName} does not exist");
            }
            else
            {
                // found the source table, so load it
                TableSource source = new TableSource(tableSource);
                Delete delete = new Delete(tableDestination, source, PredicateContext.PredicateExpressions);

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
