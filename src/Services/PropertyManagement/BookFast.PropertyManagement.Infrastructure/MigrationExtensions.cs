using BookFast.PropertyManagement.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.PropertyManagement.Infrastructure
{
    public static class MigrationExtensions
    {
        public static void ApplyMigration(IServiceScope scope)
        {
            using var context = scope.ServiceProvider.GetRequiredService<PropertyManagementContext>();

            context.Database.Migrate();
        }
    }
}
