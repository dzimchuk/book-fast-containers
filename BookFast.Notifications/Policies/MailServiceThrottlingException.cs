namespace BookFast.Notifications.Policies
{
    public class MailServiceThrottlingException : Exception
    {
        public MailServiceThrottlingException(Azure.Response response)
            : base($"Email sending tier limit is reached. Details: {response}")
        {
        }
    }
}
