using System;
namespace JankSQL
{
    public class BindResult
    {
        public BindResult()
        {
            this.BindStatus = BindStatus.NOT_BOUND;
        }

        public BindResult(BindStatus bs)
        {
            this.BindStatus = bs;
        }

        public BindResult(BindStatus bs, string errorMessage)
        {
            this.BindStatus = bs;
            this.ErrorMessage = errorMessage;
        }

        public BindStatus BindStatus { get; set; }

        public string? ErrorMessage { get; set; }

        public bool IsSuccessful => (BindStatus == BindStatus.SUCCESSFUL) || (BindStatus == BindStatus.SUCCESSFUL);

        public override string ToString()
        {
            if (BindStatus == BindStatus.FAILED || BindStatus == BindStatus.SUCCESSFUL_WITH_MESSAGE)
                return $"{BindStatus} ({ErrorMessage})";

            return $"{BindStatus}";
        }

        internal static BindResult Success()
        {
            BindResult bindResult = new BindResult(BindStatus.SUCCESSFUL);
            return bindResult;
        }

        internal static BindResult Failed(string errorMessage)
        {
            BindResult bindResult = new BindResult(BindStatus.FAILED, errorMessage);
            return bindResult;
        }

    }
}
