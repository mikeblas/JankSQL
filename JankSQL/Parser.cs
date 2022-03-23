namespace JankSQL
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
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


    internal class AntlrErrors<TToken> : IAntlrErrorListener<TToken>
        where TToken : IToken
    {
        private readonly List<string> errorList = new List<string>();

        internal List<string> ErrorList
        {
            get { return errorList; }
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, TToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            int start = offendingSymbol.StartIndex;
            int stop = offendingSymbol.StopIndex;
            string err = $"grammar syntax error on line {line}({start}:{stop}) near {offendingSymbol.Text}: {msg}";
            Console.WriteLine(err);
            errorList.Add(err);
        }
    }

    internal class DescriptiveErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        private readonly List<string> errorList = new List<string>();

        public static DescriptiveErrorListener Instance { get; } = new DescriptiveErrorListener();

        internal List<string> ErrorList
        {
            get { return errorList; }
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string sourceName = recognizer.InputStream.SourceName;
            // never ""; might be "<unknown>" == IntStreamConstants.UnknownSourceName
            sourceName = $"{sourceName}:{line}:{charPositionInLine}";
            string err = $"{sourceName}: line {line}:{charPositionInLine} {msg}";
            Console.Error.WriteLine(err);
            errorList.Add(err);

        }
    }

    /// <summary>
    /// Parses a File of SQL commands into an ExecutableBatch object.  The ExecutableBatch might indicate
    /// errors in parsing. If it does, it can't be executed. If it has no errors, it may be ex ecuted to
    /// return actual results (or effect changes to the database.)
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Helper to build and visit a parse tree produced by the given lexer.
        /// </summary>
        /// <param name="lexer">TSqlLexer initialized from the TSqlLexer.</param>
        /// <returns>an ExecutableBatch.</returns>
        private static ExecutableBatch ParseTreeFromLexer(TSqlLexer lexer)
        {
            var tokenErrorListener = DescriptiveErrorListener.Instance;
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(tokenErrorListener);

            var tokenStream = new CommonTokenStream(lexer);

            var parser = new TSqlParser(tokenStream);
            var errorListener = new AntlrErrors<IToken>();
            parser.RemoveErrorListeners();
            parser.AddErrorListener(errorListener);

            var tree = parser.tsql_file();

            ExecutionContext? context = null;

            Console.WriteLine($"{parser.NumberOfSyntaxErrors} syntax errors");
            if (parser.NumberOfSyntaxErrors == 0)
            {
                var ml = new JankListener();
                ParseTreeWalker.Default.Walk(ml, tree);
                context = ml.ExecutionContext;
            }

            ExecutableBatch batch = new ExecutableBatch(errorListener.ErrorList, tokenErrorListener.ErrorList, context);
            return batch;
        }

        /// <summary>
        /// case-sensitive parsing; this wkips the CaseChangingCharStream and requires
        /// all-upper SQL tokens. Mixed-case identifiers must be in [QuoteBrackets].
        /// Eventually, I can remove this and commit to mixed-case the parsing solution.
        /// </summary>
        /// <param name="str">SQL File to be parsed.</param>
        /// <returns>an ExecutableBatch that has errors, or can be executed.</returns>
        public static ExecutableBatch ParseSQLFileFromStringCS(string str)
        {
            var lexer = new TSqlLexer(new AntlrInputStream(str));
            return ParseTreeFromLexer(lexer);
        }

        /// <summary>
        /// Parse a SQL File from a string. (A File is a set of executable batches of
        /// statements, not necessarily a disk file.)
        /// </summary>
        /// <param name="str">String with a File of SQL batches to execute.</param>
        /// <returns>ExecutableBatch object which can be executed, or which represents parsing errors.</returns>
        public static ExecutableBatch ParseSQLFileFromString(string str)
        {
            ICharStream s = CharStreams.fromString(str);
            CaseChangingCharStream upper = new CaseChangingCharStream(s, true);
            var lexer = new TSqlLexer(upper);
            return ParseTreeFromLexer(lexer);
        }

        /// <summary>
        /// Parse a SQL File from a TextReader. (A File is a set of executable batches of
        /// statements, not necessarily a disk file.)
        /// </summary>
        /// <param name="reader">readable TextReader containing a File of SQL batches to execute.</param>
        /// <returns>ExecutableBatch object which can be executed, or which represents parsing errors.</returns>
        public static ExecutableBatch ParseSQLFileFromTextReader(TextReader reader)
        {
            ICharStream s = CharStreams.fromTextReader(reader);
            CaseChangingCharStream upper = new CaseChangingCharStream(s, true);
            var lexer = new TSqlLexer(upper);
            return ParseTreeFromLexer(lexer);
        }

        /// <summary>
        /// Parse a SQL File from a disk file. (A File is a set of executable batches of
        /// statements, not necessarily a disk file.)
        /// </summary>
        /// <param name="strFileName">String with a file name, possibly including path, to open and read.</param>
        /// <returns>ExecutableBatch object which can be executed, or which represents parsing errors.</returns>
        public static ExecutableBatch ParseSQLFileFromFileName(string strFileName)
        {
            using TextReader str = new StreamReader(strFileName);
            return Parser.ParseSQLFileFromTextReader(str);
        }

    }
}
