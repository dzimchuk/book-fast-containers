using BookFast.Booking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFast.Booking.Infrastructure.Configurations
{
    internal class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
    {
        public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
        {
            builder.HasKey(attempt => attempt.Id);
            builder.Property(attempt => attempt.Id).ValueGeneratedNever();

            builder.Property(attempt => attempt.ReservationId).IsRequired(true);
            builder.HasIndex(attempt => attempt.ReservationId).IsUnique();

            builder.Property(attempt => attempt.DueAt).IsRequired(true);
        }
    }
}
