using BookFast.Identity.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFast.Identity.Infrastructure.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedNever().HasMaxLength(36);

            builder.Property(user => user.TenantId).IsRequired(false).HasMaxLength(36);

            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(user => user.TenantId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
