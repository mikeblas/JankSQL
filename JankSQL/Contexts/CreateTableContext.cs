namespace JankSQL.Contexts
{
    using System.Collections.Immutable;

    using JankSQL.Engines;
    using JankSQL.Expressions;

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

        public object Clone()
        {
            var context = new CreateTableContext(tableName, columnNames, columnTypes);
            return context;
        }

        public BindResult Bind(Engines.IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            Console.WriteLine("WARNING: Bind() not implemented for CreateTableContext");
            return new(BindStatus.SUCCESSFUL);
        }

        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? accessor, IDictionary<string, ExpressionOperand> bindValues)
        {
            engine.CreateTable(tableName, columnNames.ToImmutableList(), columnTypes.ToImmutableList());

            ExecuteResult ret = ExecuteResult.SuccessWithMessage($"table {tableName} created");
            return ret;
        }
    }
}
