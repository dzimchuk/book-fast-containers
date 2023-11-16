using Microsoft.Extensions.Configuration;

namespace BookFast.Api.Swagger;

internal static class ConfigurationExtensions
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
        public SwaggerAuthSettings Swagger { get; set; }
    }

    public class SwaggerAuthSettings
    {
        public string ClientId { get; set; }
        public string[] Scopes { get; set; }
    }
}
