using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace BookFast.Configuration
{
    public static class ConfigurationHelper
    {
        public static string GetConnectionString(string serviceName)
        {
            var targetEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(targetEnv))
            {
                throw new ArgumentException("No target environment has been specified. Please make sure to define ASPNETCORE_ENVIRONMENT environment variable.");
            }

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), $"../{serviceName}");
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{targetEnv}.json", optional: false);

            var configuration = configurationBuilder.Build();

            if (configuration.GetSection("KeyVault").Exists())
            {
                configurationBuilder.AddAzureKeyVault();
            }
            else
            {
                var doc = XDocument.Parse(File.ReadAllText(Path.Combine(basePath, $"{serviceName}.csproj")));
                var userSecretId = (from param in doc.Descendants(XName.Get("UserSecretsId"))
                                    select param.Value).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(userSecretId))
                {
                    throw new Exception($"No user secret ID found for service '{serviceName}'.");
                }

                configurationBuilder.AddUserSecrets(userSecretId);
            }

            configuration = configurationBuilder.Build();
            var connectionString = configuration["Data:DefaultConnection:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception($"No connection string found for target environment '{targetEnv}'.");
            }

            return connectionString;
        }
    }
}
