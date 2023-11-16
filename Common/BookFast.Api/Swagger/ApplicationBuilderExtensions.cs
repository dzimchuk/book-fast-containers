using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;

namespace BookFast.Api.Swagger;

public static class ApplicationBuilderExtensions
{
    public static void UseSwaggerMiddlewareAndUI(this IApplicationBuilder app, IConfiguration configuration, string configKey = "Auth")
    {
        app.UseSwagger();

        var authSettings = configuration.GetAuthSettings(configKey);

        app.UseSwaggerUI(options =>
        {
            options.EnablePersistAuthorization();
            options.OAuthClientId(authSettings.Swagger.ClientId);
            options.OAuthUsePkce();
            options.InjectStyles();
        });
    }

    private static void InjectStyles(this SwaggerUIOptions options)
    {
        const string swaggerStyles =
@"
<style>
    .swagger-ui .auth-container .wrapper {
        display: none;
    }

    .swagger-ui .auth-container .scopes .wrapper {
        display: block;
    }
</style>
";
        var builder = new StringBuilder(options.HeadContent);
        builder.AppendLine(swaggerStyles);
        options.HeadContent = builder.ToString();
    }
}
