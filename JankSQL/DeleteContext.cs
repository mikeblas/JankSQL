
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

        internal PredicateContext PredicateContext { get { return predicateContext; } set { predicateContext = value; } }

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

        public ExecuteResult Execute()
        {
            ExecuteResult results = new ExecuteResult();

            // get systables
            Engines.DynamicCSV sysTables = new Engines.DynamicCSV("sys_tables.csv", "sys_tables");
            sysTables.Load();

            // get the file name for our table
            string? effectiveTableFileName = Engines.DynamicCSV.FileFromSysTables(sysTables, tableName.TableName);

            if (effectiveTableFileName == null)
            {
                throw new ExecutionException($"Table {tableName} does not exist");
            }
            else
            {
                // found the source table, so load it
                Engines.DynamicCSV table = new Engines.DynamicCSV(effectiveTableFileName, tableName.TableName);

                TableSource source = new TableSource(table);
                Delete delete = new Delete(table, source, predicateContext.PredicateExpressions);

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
