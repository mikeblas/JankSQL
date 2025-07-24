namespace JankSQL.Contexts
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class CreateIndexContext : IExecutableContext
    {
        private readonly List<(string columnName, bool isDescending)> columnInfo;

        internal CreateIndexContext(FullTableName tableName, string indexName, bool isUnique)
        {
            this.TableName = tableName;
            this.IndexName = indexName;
            this.IsUnique = isUnique;
            columnInfo = new List<(string, bool)>();
        }

        internal FullTableName TableName { get; }

        internal string IndexName { get; }

        internal bool IsUnique { get; }

        public object Clone()
        {
            var clone = new CreateIndexContext(TableName, IndexName, IsUnique);
            clone.columnInfo.AddRange(columnInfo);
            return clone;
        }

        public BindResult Bind(Engines.IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            Console.WriteLine("WARNING: Bind() not implemented for CreateIndexContext");
            return new(BindStatus.SUCCESSFUL);
        }

        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? accessor, IDictionary<string, ExpressionOperand> bindValues)
        {
            engine.CreateIndex(TableName, IndexName, IsUnique, columnInfo);

            //TODO: can we get row count here?
            ExecuteResult ret = ExecuteResult.SuccessWithRowsAffected(0);
            return ret;
        }

        public void Dump()
        {
            Console.WriteLine($"Create {(IsUnique ? "UNIQUE" : "non-Unique")} on table {TableName}");

            foreach (var (columnName, isDescending) in columnInfo)
                Console.WriteLine($"   {columnName}: {(isDescending ? "DESCENDING" : "ASCENDING")}");
        }

        internal void AddColumn(string columnName, bool isDescending)
        {
            columnInfo.Add((columnName, isDescending));
        }
    }
}
