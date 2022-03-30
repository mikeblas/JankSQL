namespace JankSQL.Contexts
{
    using JankSQL.Operators;

    internal enum SetOperator
    {
        ASSIGN,
        ADD_ASSIGN,
        SUB_ASSIGN,
        MUL_ASSIGN,
        DIV_ASSIGN,
        MOD_ASSIGN,
    }

    //TODO: move to Operators
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

        internal void Execute(IRowValueAccessor outputaccessor, IRowValueAccessor inputAccessor)
        {
            if (op != SetOperator.ASSIGN)
                throw new NotImplementedException();

            ExpressionOperand val = expression.Evaluate(inputAccessor);
            outputaccessor.SetValue(fcn, val);
        }
    }

    internal class UpdateContext : IExecutableContext
    {
        private readonly FullTableName tableName;
        private readonly TSqlParser.Update_statementContext context;
        private readonly List<SetOperation> setList = new ();

        private PredicateContext? predicateContext;

        internal UpdateContext(TSqlParser.Update_statementContext context, FullTableName tableName)
        {
            this.context = context;
            this.tableName = tableName;
        }

        internal PredicateContext? PredicateContext
        {
            get { return predicateContext; }
            set { predicateContext = value; }
        }

        internal FullTableName TableName
        {
            get { return tableName; }
        }

        public void Dump()
        {
            Console.WriteLine($"UPDATE {tableName}");

            Console.WriteLine("   Predicates:");
            if (PredicateContext == null || PredicateContext.PredicateExpressionListCount == 0)
            {
                Console.WriteLine("      no predicates");
            }
            else
            {
                for (int i = 0; i < PredicateContext.PredicateExpressionListCount; i++)
                {
                    Console.WriteLine($"       {PredicateContext.PredicateExpressions[i]}");
                }
            }

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

        public ExecuteResult Execute(Engines.IEngine engine)
        {
            ExecuteResult results = new ();

            Engines.IEngineTable? engineSource = engine.GetEngineTable(tableName);
            if (engineSource == null)
                throw new ExecutionException($"Table {tableName} does not exist");
            else
            {
                if (PredicateContext == null)
                    throw new InternalErrorException($"Expected predicate on UPDATE statement");

                // found the source table, so build ourselves up
                TableSource source = new (engineSource);
                Update update = new (engineSource, source, PredicateContext.PredicateExpressions, setList);

                while (true)
                {
                    ResultSet? batch = update.GetRows(5);
                    if (batch == null)
                        break;
                }

                results.ExecuteStatus = ExecuteStatus.SUCCESSFUL;
            }

            return results;
        }

        internal void AddAssignment(FullColumnName fcn, Expression x)
        {
            SetOperation op = new SetOperation(fcn, SetOperator.ASSIGN, x);
            setList.Add(op);
        }

        internal void AddAssignmentOperator(FullColumnName fcn, string op, Expression x)
        {
            throw new NotImplementedException();
        }
    }
}
