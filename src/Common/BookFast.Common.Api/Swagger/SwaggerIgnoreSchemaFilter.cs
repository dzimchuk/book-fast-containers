using BookFast.Common.Application.Messaging;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BookFast.Common.Api.Swagger
{
    internal class SwaggerIgnoreSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null || context?.Type == null)
            {
                return;
            }

            var properties = context.Type.GetProperties();
            foreach (var property in properties)
            {
                var ignoreAttrs = property.GetCustomAttributes(typeof(SwaggerIgnoreAttribute), true);
                if (ignoreAttrs != null && ignoreAttrs.Any())
                {
                    var propertyName = schema.Properties
                        .Where(prop => prop.Key.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                        .Select(prop => prop.Key).FirstOrDefault();
                    if (propertyName != null)
                    {
                        schema.Properties.Remove(propertyName);
                    }
                }
            }
        }
    }
}
