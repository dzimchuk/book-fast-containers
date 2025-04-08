using Azure;
using Azure.Communication.Email;
using Azure.Core;
using Azure.Core.Pipeline;
using BookFast.Common.Application.Integration;
using Fluid;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookFast.Common.Infrastructure.Integration
{
    public abstract class MailSender<TModel> : IConsumer<MailMessage<TModel>>
    {
        private readonly CommunicationServiceOptions options;
        private readonly ILogger logger;

        protected MailSender(IOptions<CommunicationServiceOptions> options, ILogger logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        // see https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/send-email?pivots=programming-language-csharp&tabs=linux%2Cconnection-string%2Csend-email-and-get-status-async%2Csync-client
        public async Task Consume(ConsumeContext<MailMessage<TModel>> context)
        {
            var emailClientOptions = new EmailClientOptions();
            emailClientOptions.AddPolicy(new Catch429Policy(), HttpPipelinePosition.PerRetry);

            var emailClient = new EmailClient(options.ConnectionString, emailClientOptions);

            var sender = options.MailFromAddress;

            var mailMessage = context.Message;

            var recipients = mailMessage.To.Select(emailAddress => new EmailAddress(emailAddress)).ToList();
            var subject = mailMessage.Subject;
            var htmlContent = CreateEmailBody(mailMessage.Model);

            if (htmlContent == null)
            {
                return;
            }

            var message = new EmailMessage(
                    sender,
                    new EmailRecipients(recipients),
                    new EmailContent(subject)
                    {
                        Html = htmlContent
                    });

            try
            {
                var emailSendOperation = await emailClient.SendAsync(WaitUntil.Completed, message);

                logger.LogInformation($"Email Sent. Operation ID: {emailSendOperation.Id}, Status = {emailSendOperation.Value.Status}");
            }
            catch (RequestFailedException ex)
            {
                // OperationID is contained in the exception message and can be used for troubleshooting purposes
                logger.LogError($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }
        }

        private string CreateEmailBody(TModel model)
        {
            var markup = LoadTemplate();
            if (markup == null)
            {
                return null;
            }

            var fluidParser = new FluidParser();

            if (!fluidParser.TryParse(markup, out var template, out var error))
            {
                logger.LogError($"Cannot render email template. {error}");
            }

            var context = new TemplateContext(model);
            var email = template.Render(context);

            return email;
        }

        protected abstract string LoadTemplate();
    }

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

    public class MailServiceThrottlingException : Exception
    {
        public MailServiceThrottlingException(Azure.Response response)
            : base($"Email sending tier limit is reached. Details: {response}")
        {
        }
    }
}
