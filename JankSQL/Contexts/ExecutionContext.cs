namespace JankSQL.Contexts
{
    public class ExecutionContext
    {
        private List<IExecutableContext> executeContexts = new ();

        public List<IExecutableContext> ExecuteContexts
        {
            get { return executeContexts; }
            set { executeContexts = value; }
        }

        public void Dump()
        {
            foreach (var context in executeContexts)
                context.Dump();
        }

        public ExecuteResult[] Execute(Engines.IEngine engine, Dictionary<string, ExpressionOperand> bindValues)
        {
            var clonedContexts = new List<IExecutableContext>();
            foreach (var item in executeContexts)
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
            ExecuteResult result;

            if (executeContexts.Count != 1)
                result = ExecuteResult.FailureWithError("ExecuteSingle() called on multiple-context batch");
            else
            {
                IExecutableContext clonedContext = (IExecutableContext)executeContexts[0].Clone();
                result = clonedContext.Execute(engine, null, bindValues);
            }

            return result;
        }
    }
}
