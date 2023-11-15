using Azure;
using Azure.Communication.Email;
using MassTransit;
using Microsoft.Extensions.Options;
using System.Reflection;
using Azure.Core;
using Fluid;
using BookFast.Integration;
using BookFast.Notifications.Policies;
using BookFast.Integration.Models.Identity;

namespace BookFast.Notifications
{
    public class MailSender<TModel> : IConsumer<MailMessage<TModel>>
    {
        private readonly CommunicationServiceOptions options;
        private readonly ILogger logger;

        public MailSender(IOptions<CommunicationServiceOptions> options, ILogger<MailSender<TModel>> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        // see https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/send-email?pivots=programming-language-csharp&tabs=linux%2Cconnection-string&utm_source=pocket_saves
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

                logger.LogInformation($"Email Sent. Status = {emailSendOperation.Value.Status}");

                // Get the OperationId so that it can be used for tracking the message for troubleshooting
                string operationId = emailSendOperation.Id;
                logger.LogInformation($"Email operation id = {operationId}");
            }
            catch (RequestFailedException ex)
            {
                // OperationID is contained in the exception message and can be used for troubleshooting purposes
                logger.LogError($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }
        }

        private static Dictionary<Type, string> templates = new Dictionary<Type, string>
        {
            { typeof(ConfirmEmail), "Identity_ConfirmEmail" },
            { typeof(ResetPassword), "Identity_ResetPassword" }
        };

        private static string CreateEmailBody(TModel model)
        {
            if (!templates.TryGetValue(typeof(TModel), out var templateName))
            {
                throw new InvalidOperationException($"Unknown email model type: {typeof(TModel)}");
            }

            var markup = LoadTemplate(templateName);
            var fluidParser = new FluidParser();

            if (!fluidParser.TryParse(markup, out var template, out var error))
            {
                throw new InvalidOperationException($"Cannot render email template. {error}");
            }

            var context = new TemplateContext(model);
            var email = template.Render(context);

            return email;
        }

        private static string LoadTemplate(string templateName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.EmailTemplates.{templateName}.html");

            using StreamReader reader = new(resourceStream);
            return reader.ReadToEnd();
        }
    }
}
