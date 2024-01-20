namespace JankSQL.Contexts
{
    /// <summary>
    /// An ExecutionContext provide as a context (state) for executing a set of SQL statements.
    /// This container holds the set of statements and provides methods for executing exactly one
    /// or all of the statements. ExecuteResult values are returned for each executed statement.
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        /// The list of executed statements.
        /// </summary>
        public List<IExecutableContext> ExecuteContexts { get; set; } = new();

        public void Dump()
        {
            foreach (var context in ExecuteContexts)
                context.Dump();
        }

        /// <summary>
        /// Execute all statement and return an array of ExecuteResult values about their success.
        /// </summary>
        /// <param name="engine">IEngine to be used for execution.</param>
        /// <param name="bindValues">Dictionary of bind values to provide for the executed statements.</param>
        /// <returns></returns>
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
                    BindResult br = context.Bind(engine, Array.Empty<FullColumnName>(), bindValues);
                    if (!br.IsSuccessful)
                        results.Add(ExecuteResult.FailureWithError(br.ErrorMessage));
                    else
                    {
                        ExecuteResult result = context.Execute(engine, null, bindValues);
                        results.Add(result);
                    }
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

        /// <summary>
        /// Execute a single statement and return an ExecuteResult value about the result.
        /// </summary>
        /// <param name="engine">IEngine to be used for execution.</param>
        /// <param name="bindValues">Dictionary of bind values to provide for the executed statement.</param>
        /// <returns></returns>
        public ExecuteResult ExecuteSingle(Engines.IEngine engine, Dictionary<string, ExpressionOperand> bindValues)
        {
            if (ExecuteContexts.Count != 1)
                return ExecuteResult.FailureWithError("ExecuteSingle() called on multiple-context batch");
            else
            {
                IExecutableContext clonedContext = (IExecutableContext)ExecuteContexts[0].Clone();
                BindResult br = clonedContext.Bind(engine, Array.Empty<FullColumnName>(), bindValues);
                if (!br.IsSuccessful)
                    return ExecuteResult.FailureWithError(br.ErrorMessage);
                return clonedContext.Execute(engine, null, bindValues);
            }
        }
    }
}
