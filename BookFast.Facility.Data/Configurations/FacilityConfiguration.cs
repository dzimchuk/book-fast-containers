using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFast.Facility.Data.Configurations
{
    internal class FacilityConfiguration : IEntityTypeConfiguration<Models.Facility>
    {
        public void Configure(EntityTypeBuilder<Models.Facility> builder)
        {
            builder.ToTable("Facilities", "facility");

            builder.HasKey(facility => facility.Id);
            builder.Property(facility => facility.Id).UseHiLo("facilityseq", "facility");

            builder.Property(facility => facility.Name).IsRequired(true).HasMaxLength(320);
            builder.Property(facility => facility.Description).IsRequired(false);
            builder.Property(facility => facility.Owner).IsRequired(true);
            builder.Property(facility => facility.StreetAddress).IsRequired(false);
            builder.Property(facility => facility.Latitude).IsRequired(false);
            builder.Property(facility => facility.Longitude).IsRequired(false);
            builder.Property(facility => facility.Images).IsRequired(false);
        }
    }
}
