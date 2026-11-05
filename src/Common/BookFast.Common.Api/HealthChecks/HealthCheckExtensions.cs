using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Common.Api.HealthChecks
{
    public static class HealthCheckExtensions
    {
        public const string LivenessPath = "/health/live";
        public const string ReadinessPath = "/health/ready";

        public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
        {
            services.AddHealthChecks();
            return services;
        }

        public static IEndpointRouteBuilder MapHealthCheckEndpoints(this IEndpointRouteBuilder endpoints)
        {
            // liveness deliberately runs no checks: a failing database must not get the replica restarted
            endpoints.MapHealthChecks(LivenessPath, new HealthCheckOptions
            {
                Predicate = _ => false
            }).AllowAnonymous();

            endpoints.MapHealthChecks(ReadinessPath, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            }).AllowAnonymous();

            return endpoints;
        }
    }
}
