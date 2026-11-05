using Microsoft.Extensions.Configuration;

namespace BookFast.Common.Api.Authentication
{
    public static class ConfigurationExtensions
    {
        public static AuthSettings GetAuthSettings(this IConfiguration configuration, string configKey)
        {
            var authSettings = new AuthSettings();

            if (string.IsNullOrEmpty(configKey))
            {
                configuration.Bind(authSettings);
            }
            else
            {
                configuration.Bind(configKey, authSettings);
            }

            return authSettings;
        }

        public class AuthSettings
        {
            public Uri Issuer { get; set; }

            public string[] Audiences { get; set; }

            public string[] ValidIssuers { get; set; }

            public string TokenEncryptionCertificateThumbprint { get; set; }

            public string EncryptionSymmetricKey { get; set; }

            public bool EnableAccessTokenEncryption { get; set; }
        }
    }
}
