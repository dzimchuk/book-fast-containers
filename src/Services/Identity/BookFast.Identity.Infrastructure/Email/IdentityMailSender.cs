using BookFast.Common.Infrastructure.Integration;
using BookFast.Identity.Core.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace BookFast.Identity.Infrastructure.Email
{
    internal class IdentityMailSender(IOptions<CommunicationServiceOptions> options, ILogger<IdentityMailSender> logger) 
        : MailSender(options, logger)
    {
        private static Dictionary<Type, string> templates = new()
        {
            { typeof(ConfirmEmail), "ConfirmEmail" },
            { typeof(ResetPassword), "ResetPassword" }
        };

        protected override string LoadTemplate(Type payloadType)
        {
            if (!templates.TryGetValue(payloadType, out var templateName))
            {
                logger.LogError($"Unknown email model type: {payloadType}");

                return null;
            }

            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Email.Templates.{templateName}.html");
            using var reader = new StreamReader(stream);
            
            return reader.ReadToEnd();
        }
    }
}
