namespace JankSQL.Contexts
{
    using JankSQL.Engines;
    using JankSQL.Expressions;
    using JankSQL.Operators;

    internal class DeleteContext : IExecutableContext
    {
        private readonly FullTableName tableName;

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
            Console.WriteLine("WARNING: Bind() not implemented for DeleteContext");
            return new(BindStatus.SUCCESSFUL);
        }



        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? outerAccessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            Engines.IEngineTable? tableSource = engine.GetEngineTable(tableName);

            if (tableSource == null)
            {
                throw new ExecutionException($"Table {tableName} does not exist");
            }
            else
            {
                // found the source table, so load it
                TableSource source = new (tableSource);
                Delete delete = new (tableSource, source, PredicateExpression);

                while (true)
                {
                    ResultSet batch = delete.GetRows(engine, outerAccessor, 5, bindValues);
                    if (batch.IsEOF)
                        break;
                }

                ExecuteResult results = ExecuteResult.SuccessWithRowsAffected(delete.RowsAffected);
                return results;
            }
        }
    }
}
