namespace JankSQL.Contexts
{
    using JankSQL.Engines;
    using JankSQL.Expressions;
    using JankSQL.Operators;

    internal class UpdateContext : IExecutableContext
    {
        private readonly FullTableName tableName;
        private readonly TSqlParser.Update_statementContext context;
        private readonly List<UpdateSetOperation> setList = new ();

        internal UpdateContext(TSqlParser.Update_statementContext context, FullTableName tableName)
        {
            this.context = context;
            this.tableName = tableName;
        }

        internal FullTableName TableName
        {
            get { return tableName; }
        }

        internal Expression? PredicateExpression { get; set; }

        public object Clone()
        {
            UpdateContext clone = new (context, tableName);

            clone.PredicateExpression = PredicateExpression != null ? (Expression)PredicateExpression.Clone() : null;

            clone.setList.AddRange(setList);

            return clone;
        }

        public void Dump()
        {
            Console.WriteLine($"UPDATE {tableName}");

            Console.WriteLine("   Predicates:");
            if (PredicateExpression == null)
                Console.WriteLine("      no predicates");
            else
                Console.WriteLine($"       {PredicateExpression}");

            Console.WriteLine("   Assignments:");
            if (setList == null || setList.Count == 0)
            {
                Console.WriteLine("   no assignments");
            }
            else
            {
                foreach (var op in setList)
                {
                    Console.WriteLine($"      {op}");
                }
            }
        }

        public BindResult Bind(Engines.IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            Console.WriteLine("WARNING: Bind() not implemented for UpdateContext");
            return new(BindStatus.SUCCESSFUL);
        }


        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? outerAccessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            Engines.IEngineTable? engineSource = engine.GetEngineTable(tableName);
            if (engineSource == null)
                throw new ExecutionException($"Table {tableName} does not exist");
            else
            {
                // found the source table, so build ourselves up
                TableSource source = new (engineSource);
                Update update = new (engineSource, source, PredicateExpression, setList);

                while (true)
                {
                    ResultSet batch = update.GetRows(engine, outerAccessor, 5, bindValues);
                    if (batch.IsEOF)
                        break;
                }

                ExecuteResult results = ExecuteResult.SuccessWithRowsAffected(update.RowsAffected);
                return results;
            }

        }

        internal void AddAssignment(FullColumnName fcn, Expression x)
        {
            UpdateSetOperation op = new (fcn, UpdateSetOperator.ASSIGN, x);
            setList.Add(op);
        }

        internal void AddAssignmentOperator(FullColumnName fcn, string op, Expression x)
        {
            throw new NotImplementedException();
        }
    }
}
