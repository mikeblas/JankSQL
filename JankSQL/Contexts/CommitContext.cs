namespace JankSQL.Contexts
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

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

        public BindResult Bind(Engines.IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            return BindResult.Success();
        }

        public ExecuteResult Execute(IEngine engine, IRowValueAccessor? accessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            engine.Commit();

            ExecuteResult results = ExecuteResult.SuccessWithMessage("Committed");
            return results;
        }
    }
}
