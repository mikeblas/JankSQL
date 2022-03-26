namespace JankSQL.Contexts
{
    using JankSQL.Engines;

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

        internal FullTableName TableName { get { return tableName; } }

        internal string IndexName { get { return indexName; } }

        internal bool IsUnique { get { return isUnique; } }

        public ExecuteResult Execute(IEngine engine)
        {
            engine.CreateIndex(tableName, indexName, isUnique, columnInfo);

            ExecuteResult ret = new ()
            {
                ExecuteStatus = ExecuteStatus.SUCCESSFUL,
            };
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
