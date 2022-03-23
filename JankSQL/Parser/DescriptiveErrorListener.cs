namespace JankSQL
{
    using Antlr4.Runtime;

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
}
