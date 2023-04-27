using BookFast.Identity.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFast.Identity.Infrastructure.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(user => user.TenantId).HasDefaultValue(Guid.Empty.ToString().ToLowerInvariant());

            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(user => user.TenantId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
