using System;
namespace JankSQL
{
    public class BindResult
    {
        private readonly string? errorMessage;

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
            this.errorMessage = errorMessage;
        }

        public BindStatus BindStatus { get; set; }

        public string ErrorMessage
        {
            get
            {
                if (ErrorMessage == null)
                    throw new InternalErrorException("BindStatus succeeded, but ErrorMessage referenced");
                return ErrorMessage;
            }
            /*
            set
            {
                errorMessage = value;
            }
            */
        }

        public bool IsSuccessful => (BindStatus == BindStatus.SUCCESSFUL) || (BindStatus == BindStatus.SUCCESSFUL);

        public override string ToString()
        {
            if (BindStatus == BindStatus.FAILED || BindStatus == BindStatus.SUCCESSFUL_WITH_MESSAGE)
                return $"{BindStatus} ({ErrorMessage})";

            return $"{BindStatus}";
        }

        internal static BindResult Success()
        {
            BindResult bindResult = new (BindStatus.SUCCESSFUL);
            return bindResult;
        }

        internal static BindResult Failed(string errorMessage)
        {
            BindResult bindResult = new (BindStatus.FAILED, errorMessage);
            return bindResult;
        }

    }
}
