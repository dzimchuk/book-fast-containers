using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BookFast.Integration
{
    public class ServiceBusOptions
    {
        private string prefix;
        private bool isLocalRun;

        public string ConnectionString { get; set; }
        public string MailSenderQueue { get; set; }
        public string Prefix
        {
            get
            {
                if (string.IsNullOrWhiteSpace(prefix) && isLocalRun)
                {
                    throw new Exception("ServiceBus:Prefix is required when running in Development mode.");
                }

                return prefix;
            }

            set => prefix = value;
        }

        public void SetIsLocalRun(bool isLocalRun)
        {
            this.isLocalRun = isLocalRun;
        }

        public string GetMailSenderQueueName() =>
            string.IsNullOrWhiteSpace(Prefix)
            ? MailSenderQueue
            : $"{Prefix}-{MailSenderQueue}";
    }

    public static class ConfigurationExtensions
    {
        public static ServiceBusOptions GetServiceBusOptions(this IConfiguration configuration, IHostEnvironment env)
        {
            var options = configuration.GetSection("ServiceBus").Get<ServiceBusOptions>() ?? new ServiceBusOptions();
            options.SetIsLocalRun(env.IsDevelopment());

            return options;
        }
    }
}
