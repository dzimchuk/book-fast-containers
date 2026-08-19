using BookFast.Booking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFast.Booking.Infrastructure.Configurations
{
    internal class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.HasKey(reservation => reservation.Id);
            builder.Property(reservation => reservation.Id).ValueGeneratedNever();

            builder.Property(reservation => reservation.GuestId).IsRequired(true).HasMaxLength(36);
            builder.HasIndex(reservation => reservation.GuestId);

            builder.Property(reservation => reservation.AccommodationId).IsRequired(true);
            builder.HasIndex(reservation => reservation.AccommodationId);

            builder.Property(reservation => reservation.TenantId).IsRequired(true).HasMaxLength(36);
            builder.HasIndex(reservation => reservation.TenantId);

            builder.OwnsOne(reservation => reservation.Stay, stayBuilder =>
            {
                stayBuilder.Property(stay => stay.CheckIn).HasColumnName("check_in").IsRequired(true);
                stayBuilder.Property(stay => stay.CheckOut).HasColumnName("check_out").IsRequired(true);
            });
            builder.Navigation(reservation => reservation.Stay).IsRequired();

            builder.Property(reservation => reservation.Units).IsRequired(true);

            builder.OwnsOne(reservation => reservation.Rate, rateBuilder =>
            {
                rateBuilder.Property(rate => rate.Amount).HasColumnName("rate_amount").IsRequired(true);
                rateBuilder.Property(rate => rate.Currency).HasMaxLength(3).HasColumnName("rate_currency").IsRequired(true);
            });
            builder.Navigation(reservation => reservation.Rate).IsRequired();

            builder.Property(reservation => reservation.Status).IsRequired(true).HasConversion<string>().HasMaxLength(20);

            builder.Property(reservation => reservation.ExpiresAt).IsRequired(true);

            builder.Property(reservation => reservation.RowVersion).IsRowVersion();
        }
    }
}
