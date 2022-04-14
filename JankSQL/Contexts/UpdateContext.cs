namespace JankSQL.Contexts
{
    using JankSQL.Expressions;
    using JankSQL.Operators;
    using JankSQL.Engines;

    internal enum SetOperator
    {
        ASSIGN,
        ADD_ASSIGN,
        SUB_ASSIGN,
        MUL_ASSIGN,
        DIV_ASSIGN,
        MOD_ASSIGN,
    }

    //TODO: move to Operators (or Expressions?)
    internal class SetOperation
    {
        private readonly FullColumnName fcn;
        private readonly Expression expression;
        private readonly SetOperator op;

        internal SetOperation(FullColumnName fcn, SetOperator op, Expression expression)
        {
            this.fcn = fcn;
            this.op = op;
            this.expression = expression;
        }

        public override string ToString()
        {
            return $"{fcn} {op} {expression}";
        }

        internal void Execute(Engines.IEngine engine, IRowValueAccessor outputaccessor, IRowValueAccessor inputAccessor)
        {
            if (op != SetOperator.ASSIGN)
                throw new NotImplementedException();

            ExpressionOperand val = expression.Evaluate(inputAccessor, engine);
            outputaccessor.SetValue(fcn, val);
        }
    }

    internal class UpdateContext : IExecutableContext
    {
        private readonly FullTableName tableName;
        private readonly TSqlParser.Update_statementContext context;
        private readonly List<SetOperation> setList = new ();

        private Expression? predicateExpression;

        internal UpdateContext(TSqlParser.Update_statementContext context, FullTableName tableName)
        {
            this.context = context;
            this.tableName = tableName;
        }

        internal FullTableName TableName
        {
            get { return tableName; }
        }

        internal Expression? PredicateExpression
        {
            get { return predicateExpression; }
            set { predicateExpression = value; }
        }

        public void Dump()
        {
            Console.WriteLine($"UPDATE {tableName}");

            Console.WriteLine("   Predicates:");
            if (predicateExpression == null)
                Console.WriteLine("      no predicates");
            else
                Console.WriteLine($"       {predicateExpression}");

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

        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? outerAccessor)
        {
            Engines.IEngineTable? engineSource = engine.GetEngineTable(tableName);
            if (engineSource == null)
                throw new ExecutionException($"Table {tableName} does not exist");
            else
            {
                // found the source table, so build ourselves up
                TableSource source = new (engineSource);
                Update update = new (engineSource, source, predicateExpression, setList);

                while (true)
                {
                    ResultSet batch = update.GetRows(engine, outerAccessor, 5);
                    if (batch.IsEOF)
                        break;
                }

                ExecuteResult results = ExecuteResult.SuccessWithRowsAffected(update.RowsAffected);
                return results;
            }

        }

        internal void AddAssignment(FullColumnName fcn, Expression x)
        {
            SetOperation op = new (fcn, SetOperator.ASSIGN, x);
            setList.Add(op);
        }

        internal void AddAssignmentOperator(FullColumnName fcn, string op, Expression x)
        {
            throw new NotImplementedException();
        }
    }
}
