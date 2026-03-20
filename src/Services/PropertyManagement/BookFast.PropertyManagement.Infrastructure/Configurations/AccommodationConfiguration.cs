using BookFast.PropertyManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookFast.PropertyManagement.Infrastructure.Configurations
{
    internal class AccommodationConfiguration : IEntityTypeConfiguration<Accommodation>
    {
        public void Configure(EntityTypeBuilder<Accommodation> builder)
        {
            builder.HasKey(accommodation => accommodation.Id);
            builder.Property(accommodation => accommodation.Id).ValueGeneratedNever();

            builder.Property(accommodation => accommodation.TenantId).IsRequired(true).HasMaxLength(36);

            builder.HasIndex(accommodation => accommodation.TenantId);

            builder.Property(accommodation => accommodation.Name).IsRequired(true).HasMaxLength(100);
            builder.Property(accommodation => accommodation.Description).IsRequired(false).HasMaxLength(1000);

            var converter = new ValueConverter<string[], string>(
                array => array.ToJson(),
                json => json.ToStringArray());

            builder.Property(accommodation => accommodation.Images).IsRequired(false).HasConversion(converter);

            builder.Property(accommodation => accommodation.RoomCount).IsRequired(true);

            builder.HasOne<Property>()
                .WithMany()
                .HasForeignKey(accommodation => accommodation.PropertyId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
