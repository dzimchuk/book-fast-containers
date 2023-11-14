namespace BookFast.Infrastructure.Filters
{
    public class ExcessiveMessageSizeException : Exception
    {
        public ExcessiveMessageSizeException(Type messageType, int allowedSize, int actualSize)
            : base($"Message size for {messageType.Name} exceeded allowed size. Allowed size: {allowedSize}, Actual size: {actualSize}")
        { }
    }
}
