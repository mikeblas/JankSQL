namespace JankSQL.Contexts
{
    public class ExecutionContext
    {
        private List<IExecutableContext> executeContexts = new ();

        public List<IExecutableContext> ExecuteContexts
        {
            get { return executeContexts; } set { executeContexts = value; }
        }

        public void Dump()
        {
            foreach (var context in executeContexts)
                context.Dump();
        }

        public ExecuteResult[] Execute(Engines.IEngine engine)
        {
            List<ExecuteResult> results = new ();
            foreach (IExecutableContext context in executeContexts)
            {
                try
                {
                    ExecuteResult result = context.Execute(engine);
                    results.Add(result);
                }
                catch (ExecutionException ex)
                {
                    ExecuteResult result = new ExecuteResult(ExecuteStatus.FAILED, ex.Message);
                    Console.WriteLine($"Execute exception: {ex.Message}");
                    results.Add(result);
                }
            }

            return results.ToArray();
        }

        public ExecuteResult ExecuteSingle(Engines.IEngine engine)
        {
            ExecuteResult result;
            if (executeContexts.Count != 1)
                result = new ExecuteResult(ExecuteStatus.FAILED, "ExecuteSingle() called on multiple-context batch");
            else
                result = executeContexts[0].Execute(engine);

            return result;
        }

    }
}
