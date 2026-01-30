using BookFast.Common.Infrastructure.Integration;
using BookFast.Identity.Core.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text.Json;

namespace BookFast.Identity.Infrastructure.Email
{
    internal class IdentityMailSender(IOptions<CommunicationServiceOptions> options, ILogger<IdentityMailSender> logger) 
        : MailSender(options, logger)
    {
        private static readonly Dictionary<string, Type> templates = new()
        {
            { "ConfirmEmail", typeof(ConfirmEmail) },
            { "ResetPassword", typeof(ResetPassword) }
        };

        protected override object DeserializePayload(string payloadJson, string payloadType)
        {
            if (!templates.TryGetValue(payloadType, out var templateType))
            {
                logger.LogError($"Unknown email model type: {payloadType}");

                return null;
            }

            return JsonSerializer.Deserialize(payloadJson, templateType);
        }

        protected override string LoadTemplate(string payloadType)
        {
            if (!templates.ContainsKey(payloadType))
            {
                logger.LogError($"Unknown email model type: {payloadType}");

                return null;
            }

            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Email.Templates.{payloadType}.html");
            using var reader = new StreamReader(stream);
            
            return reader.ReadToEnd();
        }
    }
}
