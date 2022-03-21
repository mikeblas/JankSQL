namespace JankSQL.Contexts
{
    internal class CreateTableContext : IExecutableContext
    {
        private readonly FullTableName tableName;
        private readonly List<FullColumnName> columnNames;
        private readonly List<ExpressionOperandType> columnTypes;

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

        public ExecuteResult Execute(Engines.IEngine engine)
        {
            engine.CreateTable(tableName, columnNames, columnTypes);

            ExecuteResult ret = new ();
            ret.ExecuteStatus = ExecuteStatus.SUCCESSFUL;
            return ret;
        }
    }
}


