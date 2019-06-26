using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace BookFast.Api.Swagger
{
    internal class SecurityRequirementsOperationFilter : IOperationFilter
    {
        private readonly IOptions<AuthorizationOptions> authorizationOptions;

        public SecurityRequirementsOperationFilter(IOptions<AuthorizationOptions> authorizationOptions)
        {
            this.authorizationOptions = authorizationOptions;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.TryGetMethodInfo(out var methodInfo))
            {
                if (methodInfo.DeclaringType.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any()
                    || methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any())
                {
                    operation.Responses.Add("401", new Response { Description = "Unauthorized" });
                    operation.Responses.Add("403", new Response { Description = "Forbidden" });
                }
            }
        }
    }
}
