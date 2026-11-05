using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;
using static BookFast.Common.Api.Authentication.ConfigurationExtensions;

namespace BookFast.Common.Api.Authentication
{
    internal static class AuthenticationExtensions
    {
        public static IServiceCollection AddOpenIddictAuthentication(this IServiceCollection services,
                                                                     IConfiguration configuration,
                                                                     string configKey = "Authentication")
        {
            services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            services.AddOpenIddict()
                .AddValidation(options =>
                {
                    var authSettings = configuration.GetAuthSettings(configKey);

                    options.SetIssuer(authSettings.Issuer);
                    options.AddAudiences(authSettings.Audiences ?? Array.Empty<string>());

                    options.ConfigureTokenEncryption(authSettings);
                    options.Configure(validationOptions =>
                    {
                        validationOptions.TokenValidationParameters.ValidIssuers = authSettings.ValidIssuers;
                    });
                    
                    options.UseSystemNetHttp();
                    options.UseAspNetCore();
                });

            return services;
        }

        private static OpenIddictValidationBuilder ConfigureTokenEncryption(this OpenIddictValidationBuilder options, AuthSettings authSettings)
        {
            if (authSettings.EnableAccessTokenEncryption)
            {
                var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(authSettings.EncryptionSymmetricKey));
                options.AddEncryptionKey(securityKey);
            }

            return options;
        }
    }
}
