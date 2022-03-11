
namespace JankSQL
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

    internal class SetOperation
    {
        FullColumnName fcn;
        Expression expression;
        SetOperator op;

        internal SetOperation(FullColumnName fcn, SetOperator op, Expression expression)
        {
            this.fcn = fcn;
            this.op = op;
            this.expression = expression;
        }

        public override string ToString()
        {
            string s = $"{fcn} {op} {expression}";
            return s;
        }
    }

    internal class UpdateContext : IExecutableContext
    {
        FullTableName tableName;
        TSqlParser.Update_statementContext context;

        List<SetOperation> setList = new();

        internal UpdateContext(TSqlParser.Update_statementContext context, FullTableName tableName)
        {
            this.context = context;
            this.tableName = tableName;
        }

        internal PredicateContext? PredicateContext { get; set; }

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

        public ExecuteResult Execute()
        {
            throw new NotImplementedException();
        }
    }
}
