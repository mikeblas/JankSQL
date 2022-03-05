
namespace JankSQL
{
    internal class DropTableContext : IExecutableContext
    {
        FullTableName tableName;

        internal DropTableContext(FullTableName tableName)
        {
            this.tableName = tableName;
        }

        public void Dump()
        {
            Console.WriteLine($"Drop table {tableName}");
        }

        public ExecuteResult Execute()
        {
            Engines.DynamicCSV.DropTable(tableName);

            ExecuteResult ret = new ExecuteResult();
            ret.ExecuteStatus = ExecuteStatus.SUCCESSFUL;
            return ret;
        }
    }
}
