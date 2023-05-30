using BookFast.Api;
using Microsoft.AspNetCore.Builder;

namespace BookFast.Api
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