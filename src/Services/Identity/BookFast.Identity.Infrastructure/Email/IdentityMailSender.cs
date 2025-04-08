using BookFast.Common.Infrastructure.Integration;
using BookFast.Identity.Core.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace BookFast.Identity.Infrastructure.Email
{
    internal class IdentityMailSender<TModel>(IOptions<CommunicationServiceOptions> options, ILogger<IdentityMailSender<TModel>> logger) 
        : MailSender<TModel>(options, logger)
    {
        private static Dictionary<Type, string> templates = new()
        {
            { typeof(ConfirmEmail), "ConfirmEmail" },
            { typeof(ResetPassword), "ResetPassword" }
        };

        protected override string LoadTemplate()
        {
            if (!templates.TryGetValue(typeof(TModel), out var templateName))
            {
                logger.LogError($"Unknown email model type: {typeof(TModel)}");

                return null;
            }

            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Email.Templates.{templateName}.html");
            using var reader = new StreamReader(stream);
            
            return reader.ReadToEnd();
        }
    }
}
