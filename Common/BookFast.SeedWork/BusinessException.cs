using System;

namespace BookFast.SeedWork
{
    public class BusinessException : Exception
    {
        public BusinessException(string errorCode)
            : this(errorCode, null, null)
        {
        }

        public BusinessException(string errorCode, string errorDescription)
            : this(errorCode, errorDescription, null)
        {
        }

        public BusinessException(string errorCode, string errorDescription, Exception innerException)
            : base(FormatMessage(errorCode, errorDescription), innerException)
        {
            ErrorCode = errorCode;
            ErrorDescription = errorDescription;
        }

        public string ErrorCode { get; }
        public string ErrorDescription { get; }

        private static string FormatMessage(string errorCode, string errorDescription)
        {
            return !string.IsNullOrEmpty(errorDescription)
                ? $"{errorCode}: {errorDescription}"
                : errorCode;
        }
    }
}
