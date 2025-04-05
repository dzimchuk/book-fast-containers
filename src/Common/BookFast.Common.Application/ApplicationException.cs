using BookFast.Common.SeedWork;

namespace BookFast.Common.Application
{
    public class ApplicationException : Exception
    {
        public ApplicationException(string requestName, Error error = default, Exception innerException = default)
        : base("Application exception", innerException)
        {
            RequestName = requestName;
            Error = error;
        }

        public string RequestName { get; }

        public Error Error { get; }
    }
}
