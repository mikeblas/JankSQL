namespace JankSQL.Contexts
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class DropTableContext : IExecutableContext
    {
        private readonly FullTableName tableName;

        internal DropTableContext(FullTableName tableName)
        {
            this.tableName = tableName;
        }

        public void Dump()
        {
            Console.WriteLine($"Drop table {tableName}");
        }

        public BindResult Bind(Engines.IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            Console.WriteLine("WARNING: Bind() not implemented for DropTableContext");
            return new(BindStatus.SUCCESSFUL);
        }


        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? accessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            engine.DropTable(tableName);

            ExecuteResult ret = ExecuteResult.SuccessWithMessage($"table {tableName} dropped");
            return ret;
        }

        public object Clone()
        {
            DropTableContext clone = new (tableName);
            return clone;
        }
    }
}
