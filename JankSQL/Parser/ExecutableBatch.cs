namespace JankSQL
{
    using ExecutionContext = JankSQL.Contexts.ExecutionContext;

    public class ExecutableBatch
    {
        private readonly List<string> tokenErrors;
        private readonly List<string> syntaxErrors;
        private readonly ExecutionContext? executionContext;
        private ExecuteResult[]? results;

        internal ExecutableBatch(List<string> tokenErrors, List<string> syntaxErrors, ExecutionContext? ec)
        {
            this.tokenErrors = tokenErrors;
            this.syntaxErrors = syntaxErrors;
            this.executionContext = ec;
        }

        /// <summary>
        /// gets a count of errors (sum of syntax errors and token errors) seen as a result of parsing this file.
        /// </summary>
        public int TotalErrors
        {
            get { return NumberOfSyntaxErrors + NumberOfTokenErrors; }
        }

        /// <summary>
        /// gets the number of syntax errors encountered when parsing this file.
        /// </summary>
        public int NumberOfSyntaxErrors
        {
            get { return (syntaxErrors == null) ? 0 : syntaxErrors.Count; }
        }

        /// <summary>
        /// Gets the number of tokenization errors encountered when parsing this file.
        /// </summary>
        public int NumberOfTokenErrors
        {
            get { return (tokenErrors == null) ? 0 : tokenErrors.Count; }
        }

        /// <summary>
        /// Dumps diagnostic and tracing information about this ExecutableBatch. Useful for
        /// showing the execution plan and state of the executable objects within.
        /// </summary>
        public void Dump()
        {
            if (executionContext == null)
                Console.WriteLine("No execution context");
            else
                executionContext.Dump();
        }

        /// <summary>
        /// Executes this batch and gets an array of ExecuteResult objects, one for each batch.
        /// </summary>
        /// <returns>array of ExecuteResults object.</returns>
        /// <exception cref="InvalidOperationException">If never successfully pasred.</exception>
        public ExecuteResult[] Execute(Engines.IEngine engine)
        {
            if (executionContext is null)
                throw new InvalidOperationException("No valid execution context");
            results = executionContext.Execute(engine);
            return results;
        }

        /// <summary>
        /// Executes a single batch and returns a single ExecuteResult object with the results of the batch.
        /// </summary>
        /// <returns>ExecuteResults object with the results of execution.</returns>
        /// <exception cref="InvalidOperationException">If never parsed.</exception>
        public ExecuteResult ExecuteSingle(Engines.IEngine engine)
        {
            if (executionContext is null)
                throw new InvalidOperationException("No valid execution context");
            results = executionContext.Execute(engine);
            return results[0];
        }

        // remove these
        [Obsolete("ExecuteSingle() is obsolete; Work towards invoking a specific engine.")]
        public ExecuteResult ExecuteSingle()
        {
            if (executionContext is null)
                throw new InvalidOperationException("No valid execution context");

            Engines.IEngine engine2 = Engines.DynamicCSVEngine.OpenExistingOnly("F:\\JankTests\\Progress");
            results = executionContext.Execute(engine2);
            return results[0];
        }
    }
}
