using BookFast.Identity.Core;
using BookFast.Identity.Core.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Identity.Infrastructure
{
    public class IdentityContext : IdentityDbContext<User, Role, string>, IDbContext
    {
        public IdentityContext(DbContextOptions<IdentityContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Register the entity sets needed by OpenIddict.
            // Note: use the generic overload if you need
            // to replace the default OpenIddict entities.
            builder.UseOpenIddict();

            builder.HasDefaultSchema("identity");

            builder.ApplyConfigurationsFromAssembly(typeof(IdentityContext).Assembly);

            builder.AddInboxStateEntity();
            builder.AddOutboxMessageEntity();
            builder.AddOutboxStateEntity();
        }
    }
}