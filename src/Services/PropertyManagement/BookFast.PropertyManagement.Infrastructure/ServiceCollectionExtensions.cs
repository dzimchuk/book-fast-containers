using Azure.Storage.Blobs;
using BookFast.Common.Infrastructure;
using BookFast.PropertyManagement.Application;
using BookFast.PropertyManagement.Application.Files;
using BookFast.PropertyManagement.Infrastructure.Database;
using BookFast.PropertyManagement.Infrastructure.Files;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BookFast.PropertyManagement.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPropertyManagementInfrastructure(this IServiceCollection services, IConfiguration configuration, string connectionStringKey = "Sql")
        {
            var connectionString = configuration.GetConnectionString(connectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringKey}' not found.");

            services.AddDbContext<PropertyManagementContext>(options =>
            {
                options.UseSqlServer(
                    connectionString,
                    sqlServerOptions => sqlServerOptions
                        .EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null) // see also https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.PropertyManagement))
                        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .UseSnakeCaseNamingConvention();
            });

            services.AddScoped<IDbContext>(sp => sp.GetRequiredService<PropertyManagementContext>());

            services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
            services.AddSingleton(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
                return new BlobContainerClient(options.ConnectionString, options.ContainerName);
            });
            services.AddSingleton<IFileTokenIssuer, BlobFileTokenIssuer>();
            services.AddHostedService<BlobContainerInitializer>();

            services.AddMassTransit(configuration, ConfigureMassTransit);

            return services;
        }

        private static void ConfigureMassTransit(IBusRegistrationConfigurator config)
        {
            config.AddEntityFrameworkOutbox<PropertyManagementContext>(outboxOptions =>
            {
                outboxOptions.QueryDelay = TimeSpan.FromMinutes(1);

                outboxOptions.UseSqlServer();
                outboxOptions.UseBusOutbox(cfg =>
                {
                    //cfg.DisableDeliveryService();
                });

                //outboxOptions.DisableInboxCleanupService();
            });
        }
    }
}
