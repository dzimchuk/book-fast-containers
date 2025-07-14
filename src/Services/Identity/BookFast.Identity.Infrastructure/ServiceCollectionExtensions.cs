using BookFast.Common.Application.Integration;
using BookFast.Common.Infrastructure;
using BookFast.Common.Infrastructure.Integration;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Email;
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
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
                        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .UseSnakeCaseNamingConvention();
            });

            services.AddScoped<IDbContext>(serviceProvider => serviceProvider.GetRequiredService<IdentityContext>());

            services.Configure<CommunicationServiceOptions>(configuration.GetSection("CommunicationService"));

            services.AddMassTransit(configuration, ConfigureConsumers);
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

        private static void ConfigureConsumers(IBusRegistrationConfigurator config)
        {
            var endpointNameFormatter = new KebabCaseEndpointNameFormatter(prefix: null, includeNamespace: false);
            config.SetEndpointNameFormatter(endpointNameFormatter);

            config.AddConsumer<IdentityMailSender<ConfirmEmail>>();
            config.AddConsumer<IdentityMailSender<ResetPassword>>();

            EndpointConvention.Map<MailMessage<ConfirmEmail>>(new Uri($"queue:{endpointNameFormatter.Consumer<IdentityMailSender<ConfirmEmail>>()}"));
            EndpointConvention.Map<MailMessage<ResetPassword>>(new Uri($"queue:{endpointNameFormatter.Consumer<IdentityMailSender<ResetPassword>>()}"));

            config.AddConfigureEndpointsCallback((name, endpointConfig) =>
            {
                endpointConfig.UseMessageRetry(r => r.Intervals(500, 1000));

                endpointConfig.ConfigureMessageTopology<MailMessage<ConfirmEmail>>(false);
                endpointConfig.ConfigureMessageTopology<MailMessage<ResetPassword>>(false);
            });
        }
    }
}
