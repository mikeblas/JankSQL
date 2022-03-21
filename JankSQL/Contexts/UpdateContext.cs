
namespace JankSQL.Contexts
{
    enum SetOperator
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
        readonly FullColumnName fcn;
        readonly Expression expression;
        readonly SetOperator op;

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
        readonly FullTableName tableName;
        readonly TSqlParser.Update_statementContext context;
        PredicateContext? predicateContext;

        readonly List<SetOperation> setList = new();

        internal UpdateContext(TSqlParser.Update_statementContext context, FullTableName tableName)
        {
            this.context = context;
            this.tableName = tableName;
        }

        internal PredicateContext PredicateContext { get { return predicateContext!; } set { predicateContext = value;  } }

        internal FullTableName TableName { get { return tableName; } }

        internal void AddAssignment(FullColumnName fcn, Expression x)
        {
            SetOperation op = new SetOperation(fcn, SetOperator.ASSIGN, x);
            setList.Add(op);
        }
        
        internal void AddAssignmentOperator(FullColumnName fcn, string op, Expression x)
        {
            throw new NotImplementedException();
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
            ExecuteResult results = new ExecuteResult();

            Engines.IEngineTable? engineSource = engine.GetEngineTable(tableName);
            if (engineSource == null)
            {
                throw new ExecutionException($"Table {tableName} does not exist");
            }
            else
            {
                // found the source table, so build ourselves up
                TableSource source = new TableSource(engineSource);
                Update update  = new Update(engineSource, source, PredicateContext.PredicateExpressions, setList);

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
    }
}
