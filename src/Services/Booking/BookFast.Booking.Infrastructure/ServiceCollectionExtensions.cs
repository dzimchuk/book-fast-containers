using BookFast.Booking.Application;
using BookFast.Booking.Application.Payments;
using BookFast.Booking.Infrastructure.Consumers;
using BookFast.Booking.Infrastructure.Database;
using BookFast.Booking.Infrastructure.Payments;
using BookFast.Common.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Booking.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBookingInfrastructure(this IServiceCollection services, IConfiguration configuration, string connectionStringKey = "Sql")
        {
            var connectionString = configuration.GetConnectionString(connectionStringKey)
                ?? throw new InvalidOperationException($"Connection string '{connectionStringKey}' not found.");

            services.AddDbContext<BookingContext>(options =>
            {
                options.UseSqlServer(
                    connectionString,
                    sqlServerOptions => sqlServerOptions
                        .EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null) // see also https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Booking))
                        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .UseSnakeCaseNamingConvention();
            });

            services.AddScoped<IDbContext>(sp => sp.GetRequiredService<BookingContext>());

            services.Configure<PaymentOptions>(configuration.GetSection("Payments"));
            services.AddScoped<IPaymentGateway, MockPaymentGateway>();
            services.AddHostedService<PaymentSettlementSweepService>();

            services.AddMassTransit(configuration, busRegistrationConfigurator => ConfigureMassTransit(busRegistrationConfigurator, configuration));

            return services;
        }

        private static void ConfigureMassTransit(IBusRegistrationConfigurator busRegistrationConfigurator, IConfiguration configuration)
        {
            busRegistrationConfigurator.AddEntityFrameworkOutbox<BookingContext>(outboxOptions =>
            {
                outboxOptions.QueryDelay = configuration.GetValue("Outbox:QueryDelay", TimeSpan.FromSeconds(30));

                outboxOptions.UseSqlServer();
                outboxOptions.UseBusOutbox(cfg =>
                {
                    //cfg.DisableDeliveryService();
                });

                //outboxOptions.DisableInboxCleanupService();
            });

            var endpointNameFormatter = new KebabCaseEndpointNameFormatter(prefix: "booking", includeNamespace: false);
            busRegistrationConfigurator.SetEndpointNameFormatter(endpointNameFormatter);

            busRegistrationConfigurator.AddConsumer<AccommodationCreatedEventConsumer>();
            busRegistrationConfigurator.AddConsumer<AccommodationUpdatedEventConsumer>();
            busRegistrationConfigurator.AddConsumer<AccommodationDeletedEventConsumer>();
            busRegistrationConfigurator.AddConsumer<PropertyDeactivatedEventConsumer>();

            busRegistrationConfigurator.AddConsumer<PaymentSettledConsumer>();

            var paymentSettledEndpointName = endpointNameFormatter.Consumer<PaymentSettledConsumer>();
            EndpointConvention.Map<PaymentSettled>(new Uri($"queue:{paymentSettledEndpointName}"));

            busRegistrationConfigurator.AddConfigureEndpointsCallback((name, endpointConfig) =>
            {
                endpointConfig.UseMessageRetry(r => r.Intervals(500, 1000));

                if (name.Equals(paymentSettledEndpointName, StringComparison.OrdinalIgnoreCase))
                {
                    endpointConfig.ConfigureMessageTopology<PaymentSettled>(false);
                }
            });
        }
    }
}
