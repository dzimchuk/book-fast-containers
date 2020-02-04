using BookFast.SeedWork.Swagger;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace BookFast.Api.Swagger
{
    internal class SwaggerIgnoreSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
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
