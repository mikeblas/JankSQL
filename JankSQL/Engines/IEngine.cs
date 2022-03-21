namespace JankSQL.Engines
{
    public interface IEngine
    {
        public void DropTable(FullTableName tableName);

        public void CreateTable(FullTableName tableName, List<FullColumnName> columnNames, List<ExpressionOperandType> columnTypes);

        public IEngineTable? GetEngineTable(FullTableName tableName);

        public IEngineTable GetSysTables();

        public IEngineTable GetSysColumns();

        public void InjectTestTable(TestTable testTable);
    }
}
