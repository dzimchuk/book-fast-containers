using BookFast.Identity.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFast.Identity.Infrastructure.Configurations
{
    internal class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.ToTable("Tenants");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).IsRequired(true).HasMaxLength(50);

            builder.Property(t => t.Name).IsRequired(true).HasMaxLength(256);
        }
    }
}
