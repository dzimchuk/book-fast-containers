using Azure.Core.Pipeline;
using Azure.Core;

namespace BookFast.Notifications.Policies
{
    // see https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/send-email-advanced/throw-exception-when-tier-limit-reached?pivots=programming-language-csharp
    internal class Catch429Policy : HttpPipelineSynchronousPolicy
    {
        public override void OnReceivedResponse(HttpMessage message)
        {
            if (message.Response.Status == 429)
            {
                throw new MailServiceThrottlingException(message.Response);
            }
            else
            {
                base.OnReceivedResponse(message);
            }
        }
    }
}
