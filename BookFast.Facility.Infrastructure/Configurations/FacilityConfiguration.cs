using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookFast.Facility.Infrastructure.Configurations
{
    internal class FacilityConfiguration : IEntityTypeConfiguration<Core.Models.Facility>
    {
        public const string SequenceName = "facilityseq";

        public void Configure(EntityTypeBuilder<Core.Models.Facility> builder)
        {
            builder.ToTable("Facilities");

            builder.HasKey(facility => facility.Id);
            builder.Property(facility => facility.Id).UseHiLo(SequenceName);

            builder.Property(facility => facility.Name).IsRequired(true).HasMaxLength(320);
            builder.Property(facility => facility.Description).IsRequired(false);
            builder.Property(facility => facility.Owner).IsRequired(true);
            builder.Property(facility => facility.StreetAddress).IsRequired(false);

            builder
                .OwnsOne(facility => facility.Location, locationBuilder =>
                {
                    locationBuilder.Property(location => location.Latitude).IsRequired(false);
                    locationBuilder.Property(location => location.Longitude).IsRequired(false);

                    locationBuilder.WithOwner();
                });
            builder.Navigation(facility => facility.Location).IsRequired(true); // necessary as all location properties are optional

            var converter = new ValueConverter<string[], string>(
                array => array.ToJson(),
                json => json.ToStringArray());

            builder.Property(facility => facility.Images).IsRequired(false).HasConversion(converter);
        }
    }
}
