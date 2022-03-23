namespace JankSQL
{
    using Antlr4.Runtime;

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
}
