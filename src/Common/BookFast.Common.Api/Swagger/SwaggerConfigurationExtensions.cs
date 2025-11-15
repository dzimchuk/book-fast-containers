using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace BookFast.Common.Api.Swagger
{
    public static class SwaggerConfigurationExtensions
    {
        public static void AddSwaggerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("OAuth", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{configuration.GetSection("Authentication:Issuer").Get<string>()}connect/authorize"),
                            TokenUrl = new Uri($"{configuration.GetSection("Authentication:Issuer").Get<string>()}connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                ["provider"] = "Access API as a facility provider",
                                ["customer"] = "Access API as a customer"
                            }
                        },
                    },
                    Type = SecuritySchemeType.OAuth2
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement()
                {
                    [new OpenApiSecuritySchemeReference("oauth2", document)] = ["provider", "customer"]
                });

                options.CustomSchemaIds(t => t.FullName.Replace("+", ".", StringComparison.OrdinalIgnoreCase));

                options.SchemaFilter<SwaggerIgnoreSchemaFilter>();
            });
        }

        public static void UseSwaggerDefaults(this IApplicationBuilder app, IConfiguration configuration)
        {
            var identityClient = configuration.GetSection("Authentication:Swagger").Get<SwaggerIdentityClient>();

            app.UseSwagger(options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
            });

            app.UseSwaggerUI(options =>
            {
                options.OAuthClientId(identityClient.ClientId);
                options.OAuthScopes(identityClient.Scopes);
                options.OAuthUsePkce();
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });
        }

        private record SwaggerIdentityClient(string ClientId, string[] Scopes);
    }
}
