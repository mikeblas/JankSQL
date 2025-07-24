namespace JankSQL.Contexts
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

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

        public BindResult Bind(Engines.IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            Console.WriteLine("WARNING: Bind() not implemented for RollbackContext");
            return new(BindStatus.SUCCESSFUL);
        }

        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? accessor, IDictionary<string, ExpressionOperand> bindValues)
        {
            engine.Rollback();

            ExecuteResult results = ExecuteResult.SuccessWithMessage("Rolled back");
            return results;
        }
    }
}
