
namespace JankSQL
{
    internal class UpdateContext : IExecutableContext
    {
        FullTableName tableName;
        TSqlParser.Update_statementContext context;

        internal UpdateContext(TSqlParser.Update_statementContext context, FullTableName tableName)
        {
            this.context = context;
            this.tableName = tableName;
        }

        internal PredicateContext? PredicateContext { get; set; }

        internal FullTableName TableName { get { return tableName; } }

        public void Dump()
        {
            Console.WriteLine($"UPDATE {tableName}");

            if (PredicateContext == null || PredicateContext.PredicateExpressionListCount == 0)
            {
                Console.WriteLine("   no predicates");
            }
            else
            {
                for (int i = 0; i < PredicateContext.PredicateExpressionListCount; i++)
                {
                    Console.WriteLine($"    {PredicateContext.PredicateExpressions[i]}");
                }
            }
        }

        public ExecuteResult Execute()
        {
            throw new NotImplementedException();
        }
    }
}
