
namespace JankSQL
{
    public class InternalErrorException : Exception
    {
        public InternalErrorException(string description)
            : base(description)
        {
        }
    }
}
