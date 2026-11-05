using BookFast.Identity.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFast.Identity.Infrastructure.Configurations
{
    internal class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever().HasMaxLength(36);

            builder.Property(t => t.Name).IsRequired(true).HasMaxLength(256);
        }
    }
}
