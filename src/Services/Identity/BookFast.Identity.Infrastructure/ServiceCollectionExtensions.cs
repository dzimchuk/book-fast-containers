using BookFast.Common.Application.Integration;
using BookFast.Common.Infrastructure;
using BookFast.Common.Infrastructure.Integration;
using BookFast.Identity.Core;
using BookFast.Identity.Infrastructure.Database;
using BookFast.Identity.Infrastructure.Email;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Identity.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration, string connectionStringKey = "Sql")
        {
            var connectionString = configuration.GetConnectionString(connectionStringKey) 
                ?? throw new InvalidOperationException($"Connection string '{connectionStringKey}' not found.");

            services.AddDbContext<IdentityContext>(options =>
            {
                options.UseSqlServer(
                    connectionString, 
                    sqlServerOptions => sqlServerOptions
                        .EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null) // see also https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
                        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .UseSnakeCaseNamingConvention();
            });

            services.AddScoped<IDbContext>(serviceProvider => serviceProvider.GetRequiredService<IdentityContext>());

            services.Configure<CommunicationServiceOptions>(configuration.GetSection("CommunicationService"));

            services.AddMassTransit(configuration, ConfigureMassTransit);
        }

        public static void AddIdentityStore(this IdentityBuilder builder)
        {
            builder.AddEntityFrameworkStores<IdentityContext>();
        }

        public static void AddOpenIddictStore(this OpenIddictCoreBuilder builder)
        {
            // Configure OpenIddict to use the Entity Framework Core stores and models.
            // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
            builder.UseEntityFrameworkCore()
                .UseDbContext<IdentityContext>();
        }

        private static void ConfigureMassTransit(IBusRegistrationConfigurator config)
        {
            config.AddEntityFrameworkOutbox<IdentityContext>(outboxOptions =>
            {
                outboxOptions.QueryDelay = TimeSpan.FromMinutes(1);

                outboxOptions.UseSqlServer();
                outboxOptions.UseBusOutbox(cfg =>
                {
                    //cfg.DisableDeliveryService();
                });

                //outboxOptions.DisableInboxCleanupService();
            });

            ConfigureConsumers(config);
        }

        private static void ConfigureConsumers(IBusRegistrationConfigurator config)
        {
            var endpointNameFormatter = new KebabCaseEndpointNameFormatter(prefix: null, includeNamespace: false);
            config.SetEndpointNameFormatter(endpointNameFormatter);

            config.AddConsumer<IdentityMailSender>();

            EndpointConvention.Map<IMailMessage>(new Uri($"queue:{endpointNameFormatter.Consumer<IdentityMailSender>()}"));

            config.AddConfigureEndpointsCallback((name, endpointConfig) =>
            {
                endpointConfig.UseMessageRetry(r => r.Intervals(500, 1000));

                endpointConfig.ConfigureMessageTopology<IMailMessage>(false);
            });
        }
    }
}
