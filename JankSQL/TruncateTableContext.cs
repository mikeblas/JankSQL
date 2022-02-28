using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal class TruncateTableContext : IExecutableContext
    {
        string tableName;

        internal TruncateTableContext(string tableName)
        {
            this.tableName = tableName;
        }

        public ExecuteResult Execute()
        {
            ExecuteResult result = new ExecuteResult();

            // get systables
            Engines.DynamicCSV sysTables = new Engines.DynamicCSV("sys_tables.csv", "sys_tables");
            sysTables.Load();


            string effectiveTableName = Program.GetEffectiveName(tableName);

            string? tableFileName = Engines.DynamicCSV.FileFromSysTables(sysTables, effectiveTableName);
            if (tableFileName == null)
            {
                Console.WriteLine($"Table {effectiveTableName} does not exist");
                result.ExecuteStatus = ExecuteStatus.FAILED;
                throw new InvalidOperationException();
            }

            Engines.DynamicCSV objectTable = new Engines.DynamicCSV(tableFileName, effectiveTableName);
            objectTable.TruncateTable();

            result.ExecuteStatus = ExecuteStatus.SUCCESSFUL;
            return result;
        }
    }
}
