
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace JankSQL
{

    public class ExecutableBatch
    {
        List<string> tokenErrors;
        List<string> syntaxErrors;
        List<string>? executionErrors;

        ExecutionContext? executionContext;

        ExecuteResult[]? results;

        internal ExecutableBatch(List<string> tokenErrors, List<string> syntaxErrors, ExecutionContext? ec)
        {
            this.tokenErrors = tokenErrors;
            this.syntaxErrors = syntaxErrors;
            this.executionContext = ec;
        }

        internal int TotalErrors { get { return NumberOfSyntaxErrors + NumberOfTokenErrors; } }

        internal int NumberOfSyntaxErrors { get { return (syntaxErrors == null) ? 0 : syntaxErrors.Count; } }

        internal int NumberOfTokenErrors { get { return (tokenErrors == null) ? 0 : tokenErrors.Count; } }

        public ExecuteResult[] Execute()
        {
            if (executionContext is null)
                throw new InvalidOperationException("No valid execution context");

            results = executionContext.Execute();
            return results;
        }

        public ExecuteResult ExecuteSingle()
        {
            if (executionContext is null)
                throw new InvalidOperationException("No valid execution context");

            results = executionContext.Execute();
            return results[0];
        }
    }


    class AntlrErrors<TToken> : IAntlrErrorListener<TToken> where TToken : IToken
    {
        List<string> errorList = new List<string>();
        internal List<string> ErrorList { get { return errorList; } }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, TToken offendingSymbol, int line, int charPositionInLine,
            string msg, RecognitionException e)
        {
            int start = offendingSymbol.StartIndex;
            int stop = offendingSymbol.StopIndex;
            string err = $"grammar syntax error on line {line}({start}:{stop}) near {offendingSymbol.Text}: {msg}";
            Console.WriteLine(err);
            errorList.Add(err);
        }
    }

    class DescriptiveErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        List<string> errorList = new List<string>();

        internal List<string> ErrorList { get { return errorList; }  }

        public static DescriptiveErrorListener Instance { get; } = new DescriptiveErrorListener();

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


    public class Parser 
    {
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


        public static ExecutableBatch ParseSQLFileFromString(string str)
        {
            var lexer = new TSqlLexer(new AntlrInputStream(str));
            return ParseTreeFromLexer(lexer);
        }

        public static ExecutableBatch ParseSQLFileFromTextReader(TextReader reader)
        {
            var lexer = new TSqlLexer(new AntlrInputStream(reader));
            return ParseTreeFromLexer(lexer);
        }

        public static ExecutableBatch ParseSQLFileFromFileName(string strFileName)
        {
            using TextReader str = new StreamReader(strFileName);
            return Parser.ParseSQLFileFromTextReader(str);
        }

    }
}
