
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
            // delete the file
            // remove from sys_columns
            // remove from sys_tables

            throw new NotImplementedException();
        }
    }
}
