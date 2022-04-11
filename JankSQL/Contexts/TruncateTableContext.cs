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
            Engines.IEngineTable? engineSource = engine.GetEngineTable(tableName);
            if (engineSource == null)
                throw new ExecutionException($"Table {tableName} does not exist");

            engineSource.TruncateTable();

            ExecuteResult result = ExecuteResult.SuccessWithMessage($"table {tableName} truncated");
            return result;
        }

        public void Dump()
        {
            Console.WriteLine("TRUNCATE TABLE of ${tableName}");
        }
    }
}
