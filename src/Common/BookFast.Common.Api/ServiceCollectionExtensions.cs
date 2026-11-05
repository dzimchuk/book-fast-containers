using BookFast.Common.Api.Authentication;
using BookFast.Common.Api.Cors;
using BookFast.Common.Api.ExceptionHandling;
using BookFast.Common.Api.OpenTelemetry;
using BookFast.Common.Api.SecurityContext;
using BookFast.Common.Application.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Common.Api
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureExceptionHandler(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
        }

        public static void AddSecurityContext(this IServiceCollection services)
        {
            services.AddScoped<ISecurityContext, SecurityContextProvider>();
        }

        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenIddictAuthentication(configuration);
        }

        public static void AddCorsServices(this IServiceCollection services, IConfiguration configuration)
        {
            CorsExtensions.AddCorsServices(services, configuration);
        }

        public static void ConfigureOpenTelemetry(this IServiceCollection services,
                                                  IConfiguration configuration,
                                                  string serviceName)
        {
            OpenTelemetryExtensions.ConfigureOpenTelemetry(services, configuration, serviceName);
        }
    }
}
