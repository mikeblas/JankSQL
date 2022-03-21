namespace JankSQL.Contexts
{
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

        public ExecuteResult Execute(Engines.IEngine engine)
        {
            engine.DropTable(tableName);

            ExecuteResult ret = new ()
            {
                ExecuteStatus = ExecuteStatus.SUCCESSFUL
            };
            return ret;
        }
    }
}
