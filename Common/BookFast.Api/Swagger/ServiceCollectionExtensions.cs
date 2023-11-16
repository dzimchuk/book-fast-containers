using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BookFast.Api.Swagger
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSwaggerServices(this IServiceCollection services, 
            IConfiguration configuration, 
            string configKey = "Auth",
            string xmlDocFileName = null)
        {
            services.AddEndpointsApiExplorer();

            var authSettings = configuration.GetAuthSettings(configKey);

            services.AddSwaggerGen(options =>
            {
                //options.SwaggerDoc(version, new OpenApiInfo
                //{
                //    Title = title,
                //    Version = version
                //});

                //options.EnableAnnotations();

                //options.OperationFilter<SecurityRequirementsOperationFilter>();

                //options.SchemaFilter<SwaggerIgnoreSchemaFilter>();

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

            if (!string.IsNullOrWhiteSpace(xmlDocFileName))
            {
                AddXmlComments(services, xmlDocFileName);
            }
        }

        private static void AddXmlComments(IServiceCollection services, string xmlDocFileName)
        {
            var xmlDoc = Path.Combine(AppContext.BaseDirectory, xmlDocFileName);

            if (!File.Exists(xmlDoc))
            {
                return;
            }

            services.ConfigureSwaggerGen(options =>
            {
                options.IncludeXmlComments(xmlDoc);
            });
        }
    }
}
