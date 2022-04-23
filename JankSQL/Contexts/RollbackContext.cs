using JankSQL.Engines;
using JankSQL.Expressions;

namespace JankSQL.Contexts
{
    internal class RollbackContext : IExecutableContext
    {
        public object Clone()
        {
            return new RollbackContext();
        }

        public void Dump()
        {
            Console.WriteLine("ROLLBACK");
        }

        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? accessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            // engine.Rollback();

            ExecuteResult results = ExecuteResult.SuccessWithMessage("Rolled back");
            return results;
        }
    }
}
