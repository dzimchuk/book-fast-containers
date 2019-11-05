using BookFast.Booking.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFast.Booking.Data.Configurations
{
    internal class BookingRecordConfiguration : IEntityTypeConfiguration<BookingRecord>
    {
        public void Configure(EntityTypeBuilder<BookingRecord> builder)
        {
            builder.ToTable("BookingRecords", "booking");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.User).IsRequired(true);

            builder.Property(entity => entity.AccommodationName).IsRequired(true).HasMaxLength(320);
            builder.Property(entity => entity.FacilityName).IsRequired(true).HasMaxLength(320);
        }
    }
}
