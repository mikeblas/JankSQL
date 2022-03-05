

namespace JankSQL
{
    internal class CreateTableContext : IExecutableContext
    {
        FullTableName tableName;
        List<FullColumnName> columnNames;
        List<ExpressionOperandType> columnTypes;

        internal CreateTableContext(FullTableName tableName, List<FullColumnName> columnNames, List<ExpressionOperandType> columnTypes)
        {
            this.tableName = tableName;
            this.columnNames = columnNames;
            this.columnTypes = columnTypes;
        }

        public void Dump()
        {
            Console.WriteLine($"CreateTable named {tableName}");
            for (int i = 0; i < columnNames.Count; i++)
            {
                Console.WriteLine($"    name: '{columnNames[i]}', type: '{columnTypes[i]}");
            }
        }

        public ExecuteResult Execute()
        {
            Engines.DynamicCSV.CreateTable(tableName, columnNames, columnTypes);

            ExecuteResult ret = new ExecuteResult();
            ret.ExecuteStatus = ExecuteStatus.SUCCESSFUL;
            return ret;
        }
    }
}


