namespace JankSQL.Contexts
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class TruncateTableContext : IExecutableContext
    {
        private readonly FullTableName tableName;

        internal TruncateTableContext(FullTableName tableName)
        {
            this.tableName = tableName;
        }

        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? accessor, Dictionary<string, ExpressionOperand> bindValues)
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

        public object Clone()
        {
            TruncateTableContext clone = new TruncateTableContext(tableName);
            return clone;
        }
    }
}
