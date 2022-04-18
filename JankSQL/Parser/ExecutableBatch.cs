namespace JankSQL
{
    using ExecutionContext = JankSQL.Contexts.ExecutionContext;

    public class ExecutableBatch
    {
        private readonly List<string> tokenErrors;
        private readonly List<string> syntaxErrors;
        private readonly ExecutionContext? executionContext;
        private readonly string? semanticError;

        private readonly Dictionary<string, ExpressionOperand> bindValues = new Dictionary<string, ExpressionOperand>(StringComparer.InvariantCultureIgnoreCase);

        private ExecuteResult[]? results;

        internal ExecutableBatch(List<string> tokenErrors, List<string> syntaxErrors, string? semanticError, ExecutionContext? ec)
        {
            this.tokenErrors = tokenErrors;
            this.syntaxErrors = syntaxErrors;
            this.semanticError = semanticError;
            executionContext = ec;
        }

        /// <summary>
        /// gets a count of errors (sum of syntax errors and token errors) seen as a result of parsing this file.
        /// </summary>
        public int TotalErrors
        {
            get { return NumberOfSyntaxErrors + NumberOfTokenErrors + (semanticError == null ? 0 : 1); }
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
        /// Gets a value indicating whether there was a semantic error.
        /// </summary>
        public bool HadSemanticError
        {
            get { return semanticError != null; }
        }

        /// <summary>
        /// Gets the semantic error string, if one was encountered.
        /// </summary>
        public string? SemanticError
        {
            get { return semanticError; }
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
            results = executionContext.Execute(engine, bindValues);
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
            results = executionContext.Execute(engine, bindValues);
            return results[0];
        }

        public void SetBindValue(string bindTargetName, ExpressionOperand bindValue)
        {
            bindValues[bindTargetName] = bindValue;
        }

        public void SetBindValue(string bindTargetName, int bindValue)
        {
            SetBindValue(bindTargetName, ExpressionOperand.IntegerFromInt(bindValue));
        }

        public void SetBindValue(string bindTargetName, string bindValue)
        {
            SetBindValue(bindTargetName, ExpressionOperand.VARCHARFromString(bindValue));
        }

        public void SetBindValue(string bindTargetName, double bindValue)
        {
            SetBindValue(bindTargetName, ExpressionOperand.DecimalFromDouble(bindValue));
        }
    }
}
