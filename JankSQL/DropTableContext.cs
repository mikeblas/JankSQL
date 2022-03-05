
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
            Engines.DynamicCSV sysTables = new Engines.DynamicCSV("sys_tables.csv", "sys_tables");
            sysTables.Load();

            DynamicCSV.FileFromSysTables(sysTables, tableName.TableName);


            Engines.DynamicCSV syscolumns = new Engines.DynamicCSV("sys_columns.csv", "sys_columns");
            syscolumns.Load();

            

            // delete the file
            // remove from sys_columns
            // remove from sys_tables

            throw new NotImplementedException();
        }
    }
}
