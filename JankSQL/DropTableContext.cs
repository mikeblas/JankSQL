
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
            Engines.DynamicCSV sysTables = new Engines.DynamicCSV("sys_tables.csv", "sys_tables");
            sysTables.Load();

            string? fileName = Engines.DynamicCSV.FileFromSysTables(sysTables, tableName.TableName);
            if (fileName == null)
                throw new ExecutionException($"Table {tableName} does not exist");

            File.Delete(fileName);

            // remove entries from sys_columns
            Engines.DynamicCSV sysColumns = new Engines.DynamicCSV("sys_columns.csv", "sys_columns");
            sysColumns.Load();

            List<int> rowIndexesToDelete = new();

            for (int i = 0; i < sysColumns.RowCount; i++)
            {

            }

            

            // remove from sys_columns


            // remove from sys_tables

            throw new NotImplementedException();
        }
    }
}
