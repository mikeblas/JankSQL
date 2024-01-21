namespace JankSQL.Contexts
{
    using JankSQL.Engines;
    using JankSQL.Expressions;
    using JankSQL.Operators;

    internal class DeleteContext : IExecutableContext
    {
        private readonly FullTableName tableName;
        private Delete? deleteOperator;

        internal DeleteContext(FullTableName tableName)
        {
            this.tableName = tableName;
        }

        internal Expression? PredicateExpression { get; set; }

        public object Clone()
        {
            DeleteContext clone = new (tableName);
            clone.PredicateExpression = PredicateExpression != null ? (Expression?)PredicateExpression.Clone() : null;
            return clone;
        }

        public void Dump()
        {
            Console.WriteLine($"DELETE FROM {tableName}");

            if (PredicateExpression == null)
                Console.WriteLine("   no predicate");
            else
                Console.WriteLine($"       {PredicateExpression}");
        }

        public BindResult Bind(Engines.IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            Engines.IEngineTable? tableSource = engine.GetEngineTable(tableName);

            if (tableSource == null)
            {
                throw new ExecutionException($"Table {tableName} does not exist");
            }
            else
            {
                // found the source table, so load it
                TableSource source = new(tableSource);
                deleteOperator = new(tableSource, source, PredicateExpression);
                BindResult br = deleteOperator.Bind(engine, outerColumnNames, bindValues);
                return br;
            }
        }

        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? outerAccessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            if (deleteOperator == null)
                throw new InternalErrorException("DeleteOperator was not bound");

            while (true)
            {
                ResultSet batch = deleteOperator.GetRows(engine, outerAccessor, 5, bindValues);
                if (batch.IsEOF)
                    break;
            }

            ExecuteResult results = ExecuteResult.SuccessWithRowsAffected(deleteOperator.RowsAffected);
            return results;
        }
    }
}
