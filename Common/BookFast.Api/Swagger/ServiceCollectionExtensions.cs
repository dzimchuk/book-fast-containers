using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BookFast.Api.Swagger
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSwaggerServices(this IServiceCollection services, IConfiguration configuration, string configKey = "Auth")
        {
            services.AddEndpointsApiExplorer();

            var authSettings = configuration.GetAuthSettings(configKey);

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{authSettings.Issuer}connect/authorize"),
                            TokenUrl = new Uri($"{authSettings.Issuer}connect/token"),
                            Scopes = authSettings.Swagger.Scopes.ToDictionary(key => key, value => value),
                        }
                    }
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });
        }
    }
}
