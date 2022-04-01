namespace JankSQL
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using ExecutionContext = JankSQL.Contexts.ExecutionContext;

    /// <summary>
    /// Parses a File of SQL commands into an ExecutableBatch object.  The ExecutableBatch might indicate
    /// errors in parsing. If it does, it can't be executed. If it has no errors, it may be ex ecuted to
    /// return actual results (or effect changes to the database.)
    /// </summary>
    public class Parser
    {
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

        public static ExecutableBatch QuietParseSQLFileFromString(string str)
        {
            ICharStream s = CharStreams.fromString(str);
            CaseChangingCharStream upper = new CaseChangingCharStream(s, true);
            var lexer = new TSqlLexer(upper);
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

            if (parser.NumberOfSyntaxErrors == 0)
            {
                var ml = new JankListener(quiet);
                ParseTreeWalker.Default.Walk(ml, tree);
                context = ml.ExecutionContext;
            }

            ExecutableBatch batch = new (errorListener.ErrorList, tokenErrorListener.ErrorList, context);
            return batch;
        }
    }
}
