using BookFast.Api;
using Microsoft.AspNetCore.Builder;

#pragma warning disable ET002 // Namespace does not match file path or default namespace
namespace BookFast.Api
#pragma warning restore ET002 // Namespace does not match file path or default namespace
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseSwagger(this IApplicationBuilder app, string title, string version)
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "api-docs/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/api-docs/{version}/swagger.json", $"{title} {version}");
                options.RoutePrefix = "api-docs";
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });
        }
    }
}