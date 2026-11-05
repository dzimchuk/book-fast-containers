using BookFast.Identity.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Identity.Infrastructure
{
    public static class MigrationExtensions
    {
        public static void ApplyMigration(IServiceScope scope)
        {
            using var context = scope.ServiceProvider.GetRequiredService<IdentityContext>();

            context.Database.Migrate();
        }
    }
}
