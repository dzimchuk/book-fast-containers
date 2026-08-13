using BookFast.Booking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFast.Booking.Infrastructure.Configurations
{
    internal class AccommodationConfiguration : IEntityTypeConfiguration<Accommodation>
    {
        public void Configure(EntityTypeBuilder<Accommodation> builder)
        {
            builder.HasKey(accommodation => accommodation.Id);
            builder.Property(accommodation => accommodation.Id).ValueGeneratedNever();

            builder.Property(accommodation => accommodation.TenantId).IsRequired(true).HasMaxLength(36);
            builder.HasIndex(accommodation => accommodation.TenantId);

            builder.HasIndex(accommodation => accommodation.PropertyId);

            builder.Property(accommodation => accommodation.Quantity).IsRequired(true);
            builder.Property(accommodation => accommodation.Active).IsRequired(true);
            builder.Property(accommodation => accommodation.OccurredAt).IsRequired(true);

            builder.OwnsOne(accommodation => accommodation.Rate, rateBuilder =>
            {
                rateBuilder.Property(rate => rate.Amount).HasColumnName("rate_amount");
                rateBuilder.Property(rate => rate.Currency).HasMaxLength(3).HasColumnName("rate_currency");
            });
        }
    }
}
