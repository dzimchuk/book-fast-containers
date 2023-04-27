namespace BookFast.SeedWork
{
    public record BusinessError(string Code, string Description);

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

        public BusinessException(IEnumerable<BusinessError> errors)
            : this(errors, null)
        {
        }

        public BusinessException(string errorCode, string errorDescription, Exception innerException)
            : this(new[] { new BusinessError(errorCode, errorDescription) }, innerException)
        {
        }

        public BusinessException(IEnumerable<BusinessError> errors, Exception innerException)
            : base("Application error(s) occured.", innerException)
        {
            Errors = errors.ToArray();
        }

        public BusinessError[] Errors { get; }
    }
}
