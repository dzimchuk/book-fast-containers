using BookFast.Security;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Api.SecurityContext
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSecurityContext(this IServiceCollection services)
        {
            services.AddScoped<ISecurityContext, SecurityContextProvider>();
        }
    }
}
