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
            var identityClient = configuration.GetSection("Authentication:Swagger").Get<SwaggerIdentityClient>();

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
                            AuthorizationUrl = new Uri(new Uri(identityClient.Issuer), "connect/authorize"),
                            TokenUrl = new Uri(new Uri(identityClient.Issuer), "connect/token"),
                            Scopes = identityClient.ScopeDefinitions
                        },
                    },
                    Type = SecuritySchemeType.OAuth2
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement()
                {
                    [new OpenApiSecuritySchemeReference("OAuth", document)] = ["provider", "customer"]
                });

                options.CustomSchemaIds(t => t.FullName.Replace("+", ".", StringComparison.OrdinalIgnoreCase));

                options.SchemaFilter<SwaggerIgnoreSchemaFilter>();

                options.ResolveConflictingActions(apiDescriptions =>
                {
                    return apiDescriptions.First();
                });
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

        private record SwaggerIdentityClient(string Issuer, string ClientId, string[] Scopes, Dictionary<string, string> ScopeDefinitions);
    }
}
