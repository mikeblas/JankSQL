namespace JankSQL.Contexts
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class DropTableContext : IExecutableContext
    {
        private readonly FullTableName tableName;

        internal DropTableContext(FullTableName tableName)
        {
            this.tableName = tableName;
        }

        public void Dump()
        {
            Console.WriteLine($"Drop table {tableName}");
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? accessor)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            engine.DropTable(tableName);

            ExecuteResult ret = ExecuteResult.SuccessWithMessage($"table {tableName} dropped");
            return ret;
        }
    }
}
