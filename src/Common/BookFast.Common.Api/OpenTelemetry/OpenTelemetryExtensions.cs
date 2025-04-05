using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BookFast.Common.Api.OpenTelemetry
{
    internal static class OpenTelemetryExtensions
    {
        public static void ConfigureOpenTelemetry(this IServiceCollection services,
                                                  IConfiguration configuration,
                                                  string serviceName)
        {
            services
                .AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);
                })
                .WithLogging(logging =>
                {
                }, 
                options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;

                    options.AttachLogsToActivityEvent();
                });

            services.AddOpenTelemetryExporters(configuration);
        }

        private static void AddOpenTelemetryExporters(this IServiceCollection services, IConfiguration configuration)
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            if (useOtlpExporter)
            {
                services.AddOpenTelemetry().UseOtlpExporter();
            }

            bool useAzureMonitor = 
                !string.IsNullOrWhiteSpace(configuration["AzureMonitor:ConnectionString"]) ||
                !string.IsNullOrWhiteSpace(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
            
            if (useAzureMonitor)
            {
                services.AddOpenTelemetry().UseAzureMonitor();
            }
        }
    }
}
