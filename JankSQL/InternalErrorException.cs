namespace JankSQL
{
    /// <summary>
    /// An InternalErrorException represents a situation that the engine
    /// or the parser can't handle. Maybe it will handle that situation
    /// some day, but for now we just don't know how to handle it.
    /// </summary>
    public class InternalErrorException : Exception
    {
        public InternalErrorException(string description)
            : base(description)
        {
        }
    }
}
