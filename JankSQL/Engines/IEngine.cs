
namespace JankSQL.Engines
{
    public interface IEngine
    {

        public void DropTable(FullTableName tableName);

        public void CreateTable(FullTableName tableName, List<FullColumnName> columnNames, List<ExpressionOperandType> columnTypes);

        public IEngineSource? GetSourceTable(FullTableName tableName);

        public IEngineDestination? GetDestinationTable(FullTableName tableName);

        public DynamicCSVTable GetSysTables();

        public DynamicCSVTable GetSysColumns();
    }
}
