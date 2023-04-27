using BookFast.Identity.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Identity.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddIdentityDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("IdentitySqlConnection") ?? throw new InvalidOperationException("Connection string 'IdentitySqlConnection' not found.");
            services.AddDbContext<IdentityContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<IDbContext>(serviceProvider => serviceProvider.GetRequiredService<IdentityContext>());
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
    }
}
