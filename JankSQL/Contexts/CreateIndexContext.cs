namespace JankSQL.Contexts
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class CreateIndexContext : IExecutableContext
    {
        private readonly FullTableName tableName;
        private readonly string indexName;
        private readonly bool isUnique;
        private readonly List<(string columnName, bool isDescending)> columnInfo;

        internal CreateIndexContext(FullTableName tableName, string indexName, bool isUnique)
        {
            this.tableName = tableName;
            this.indexName = indexName;
            this.isUnique = isUnique;
            columnInfo = new List<(string, bool)>();
        }

        internal FullTableName TableName
        {
            get { return tableName; }
        }

        internal string IndexName
        {
            get { return indexName; }
        }

        internal bool IsUnique
        {
            get { return isUnique; }
        }

        public object Clone()
        {
            var clone = new CreateIndexContext(tableName, indexName, isUnique);
            foreach (var t in columnInfo)
                clone.columnInfo.Add(t);
            return clone;
        }

        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? accessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            engine.CreateIndex(tableName, indexName, isUnique, columnInfo);

            //TODO: can we get rowcount here?
            ExecuteResult ret = ExecuteResult.SuccessWithRowsAffected(0);
            return ret;
        }

        public void Dump()
        {
            Console.WriteLine($"Create {(isUnique ? "UNIQUE" : "non-Unique")} on table {tableName}");

            foreach (var t in columnInfo)
                Console.WriteLine($"   {t.columnName}: {(t.isDescending ? "DESCENDING" : "ASCENDING")}");
        }

        internal void AddColumn(string columnName, bool isDescending)
        {
            columnInfo.Add((columnName, isDescending));
        }
    }
}
