namespace JankSQL.Contexts
{
    internal class TruncateTableContext : IExecutableContext
    {
        private readonly FullTableName tableName;

        internal TruncateTableContext(FullTableName tableName)
        {
            this.tableName = tableName;
        }

        public ExecuteResult Execute(Engines.IEngine engine)
        {
            ExecuteResult result = new ExecuteResult();


            Engines.IEngineTable? engineSource = engine.GetEngineTable(tableName);
            if (engineSource == null)
            {
                result.ExecuteStatus = ExecuteStatus.FAILED;
                throw new ExecutionException($"Table {tableName} does not exist");
            }

            engineSource.TruncateTable();

            result.ExecuteStatus = ExecuteStatus.SUCCESSFUL;
            return result;
        }

        public void Dump()
        {
            Console.WriteLine("TRUNCATE TABLE of ${tableName}");
        }
    }
}
