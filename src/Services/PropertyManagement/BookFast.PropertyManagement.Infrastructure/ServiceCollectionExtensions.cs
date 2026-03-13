using BookFast.PropertyManagement.Application;
using BookFast.PropertyManagement.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }
    }
}
