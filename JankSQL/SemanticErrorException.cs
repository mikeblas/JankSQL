namespace JankSQL
{
    /// <summary>
    /// A SemanticErrorException reprsents a semantic error: an error that results
    /// from correct syntax, but an invalid semantic. For example, invoking
    /// an unknown function name.
    /// </summary>
    public class SemanticErrorException : Exception
    {
        public SemanticErrorException(string description)
            : base(description)
        {
        }
    }
}
