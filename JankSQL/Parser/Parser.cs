namespace JankSQL
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using ExecutionContext = JankSQL.Contexts.ExecutionContext;

    /// <summary>
    /// Parses a File of SQL commands into an ExecutableBatch object.  The ExecutableBatch might indicate
    /// errors in parsing. If it does, it can't be executed. If it has no errors, it may be executed to
    /// return actual results (or effect changes to the database.)
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Parse a SQL File from a string. (A File is a set of executable batches of
        /// statements, not necessarily a disk file.)
        /// </summary>
        /// <param name="str">String with a File of SQL batches to execute.</param>
        /// <returns>ExecutableBatch object which can be executed, or which represents parsing errors.</returns>
        public static ExecutableBatch ParseSQLFileFromString(string str)
        {
            var lexer = new TSqlLexer(new AntlrInputStream(str));
            return ParseTreeFromLexer(lexer);

        }

        public static ExecutableBatch QuietParseSQLFileFromString(string str)
        {
            var lexer = new TSqlLexer(new AntlrInputStream(str));
            return QuietParseTreeFromLexer(lexer);
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
            var lexer = new TSqlLexer(s);
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

        /// <summary>
        /// Helper to build and visit a parse tree produced by the given lexer.
        /// </summary>
        /// <param name="lexer">TSqlLexer initialized from the TSqlLexer.</param>
        /// <returns>an ExecutableBatch.</returns>
        private static ExecutableBatch ParseTreeFromLexer(TSqlLexer lexer)
        {
            return ParseTreeFromLexer(lexer, false);
        }

        private static ExecutableBatch QuietParseTreeFromLexer(TSqlLexer lexer)
        {
            return ParseTreeFromLexer(lexer, true);
        }

        private static ExecutableBatch ParseTreeFromLexer(TSqlLexer lexer, bool quiet)
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
            string? semanticErrorMessage = null;

            if (parser.NumberOfSyntaxErrors == 0)
            {
                try
                {
                    var ml = new JankListener(quiet);
                    ParseTreeWalker.Default.Walk(ml, tree);
                    context = ml.ExecutionContext;
                }
                catch (SemanticErrorException e)
                {
                   semanticErrorMessage = e.Message;
                }
            }

            ExecutableBatch batch = new (errorListener.ErrorList, tokenErrorListener.ErrorList, semanticErrorMessage, context);
            return batch;
        }
    }
}
