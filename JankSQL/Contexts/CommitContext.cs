using JankSQL.Engines;
using JankSQL.Expressions;

namespace JankSQL.Contexts
{
    internal class CommitContext : IExecutableContext
    {
        public object Clone()
        {
            return new CommitContext();
        }

        public void Dump()
        {
            Console.WriteLine("CommitContext");
        }

        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? accessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            // engine.Rollback();

            ExecuteResult results = ExecuteResult.SuccessWithMessage("Committed");
            return results;
        }
    }
}
