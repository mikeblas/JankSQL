namespace JankSQL.Contexts
{
    public class ExecutionContext
    {
        public List<IExecutableContext> ExecuteContexts { get; set; } = new();

        public void Dump()
        {
            foreach (var context in ExecuteContexts)
                context.Dump();
        }

        public ExecuteResult[] Execute(Engines.IEngine engine, Dictionary<string, ExpressionOperand> bindValues)
        {
            var clonedContexts = new List<IExecutableContext>();
            foreach (var item in ExecuteContexts)
                clonedContexts.Add((IExecutableContext)item.Clone());

            List<ExecuteResult> results = new ();
            foreach (IExecutableContext context in clonedContexts)
            {
                try
                {
                    ExecuteResult result = context.Execute(engine, null, bindValues);
                    results.Add(result);
                }
                catch (ExecutionException ex)
                {
                    ExecuteResult result = ExecuteResult.FailureWithError(ex.Message);
                    Console.WriteLine($"Execute exception: {ex.Message}");
                    results.Add(result);
                }
            }

            return results.ToArray();
        }

        public ExecuteResult ExecuteSingle(Engines.IEngine engine, Dictionary<string, ExpressionOperand> bindValues)
        {
            if (ExecuteContexts.Count != 1)
                return ExecuteResult.FailureWithError("ExecuteSingle() called on multiple-context batch");
            else
            {
                IExecutableContext clonedContext = (IExecutableContext)ExecuteContexts[0].Clone();
                return clonedContext.Execute(engine, null, bindValues);
            }
        }
    }
}
