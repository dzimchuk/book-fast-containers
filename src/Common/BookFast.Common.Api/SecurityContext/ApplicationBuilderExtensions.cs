using Microsoft.AspNetCore.Builder;

namespace BookFast.Common.Api.SecurityContext
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseSecurityContext(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<SecurityContextMiddleware>();
        }
    }
}
