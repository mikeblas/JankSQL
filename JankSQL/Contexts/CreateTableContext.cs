namespace JankSQL.Contexts
{
    using JankSQL.Engines;
    using JankSQL.Expressions;
    using System.Collections.Immutable;

    internal class CreateTableContext : IExecutableContext
    {
        private readonly FullTableName tableName;
        private readonly IList<FullColumnName> columnNames;
        private readonly IList<ExpressionOperandType> columnTypes;

        internal CreateTableContext(FullTableName tableName, IList<FullColumnName> columnNames, IList<ExpressionOperandType> columnTypes)
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

        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? accessor)
        {
            engine.CreateTable(tableName, columnNames.ToImmutableList(), columnTypes.ToImmutableList());

            ExecuteResult ret = ExecuteResult.SuccessWithMessage($"table {tableName} created");
            return ret;
        }
    }
}


