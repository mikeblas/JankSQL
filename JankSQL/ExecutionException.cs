namespace JankSQL
{
    /// <summary>
    /// An ExecutionException is thrown when something goes wrong at execution;
    /// this is different than a semantic or syntactic error because it isn't
    /// predictable until the oparation actually executes.
    /// </summary>
    public class ExecutionException : Exception
    {
        public ExecutionException(string description)
            : base(description)
        {
        }
    }
}
