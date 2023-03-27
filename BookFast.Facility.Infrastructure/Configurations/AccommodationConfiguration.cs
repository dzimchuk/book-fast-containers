using BookFast.Facility.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFast.Facility.Infrastructure.Configurations
{
    internal class AccommodationConfiguration : IEntityTypeConfiguration<Accommodation>
    {
        public const string SequenceName = "accommodationseq";

        public void Configure(EntityTypeBuilder<Accommodation> builder)
        {
            builder.ToTable("Accommodations");

            builder.HasKey(accommodation => accommodation.Id);
            builder.Property(accommodation => accommodation.Id).UseHiLo(SequenceName);

            builder.Property(accommodation => accommodation.Name).IsRequired(true).HasMaxLength(320);
            builder.Property(accommodation => accommodation.Description).IsRequired(false);
            builder.Property(accommodation => accommodation.Images).IsRequired(false);

            builder.Property(accommodation => accommodation.RoomCount).IsRequired(true);

            builder.HasOne<Core.Models.Facility>()
                .WithMany()
                .HasForeignKey(accommodation => accommodation.FacilityId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
