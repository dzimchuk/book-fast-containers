using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookFast.PropertyManagement.Infrastructure.Configurations
{
    internal class PropertyConfiguration : IEntityTypeConfiguration<Core.Models.Property>
    {
        public const string SequenceName = "propertyseq";

        public void Configure(EntityTypeBuilder<Core.Models.Property> builder)
        {
            builder.ToTable("Properties");

            builder.HasKey(prop => prop.Id);
            builder.Property(prop => prop.Id).UseHiLo(SequenceName);

            builder.Property(prop => prop.Name).IsRequired(true).HasMaxLength(100);
            builder.Property(prop => prop.Description).IsRequired(false).HasMaxLength(1000);
            builder.Property(prop => prop.Owner).IsRequired(true);

            builder
                .OwnsOne(p => p.Address, addressBuilder =>
                {
                    addressBuilder.Property(addr => addr.Country).IsRequired(true).HasMaxLength(100);
                    addressBuilder.Property(addr => addr.State).IsRequired(true).HasMaxLength(100);
                    addressBuilder.Property(addr => addr.City).IsRequired(true).HasMaxLength(100);
                    addressBuilder.Property(addr => addr.Street).IsRequired(true).HasMaxLength(100);
                    addressBuilder.Property(addr => addr.ZipCode).IsRequired(true).HasMaxLength(100);

                    addressBuilder.WithOwner();
                });
            //builder.Navigation(prop => prop.Address).IsRequired(true);

            builder
                .OwnsOne(prop => prop.Location, locationBuilder =>
                {
                    locationBuilder.Property(location => location.Latitude).IsRequired(false);
                    locationBuilder.Property(location => location.Longitude).IsRequired(false);

                    locationBuilder.WithOwner();
                });
            builder.Navigation(prop => prop.Location).IsRequired(true); // necessary as all location properties are optional

            var converter = new ValueConverter<string[], string>(
                array => array.ToJson(),
                json => json.ToStringArray());

            builder.Property(prop => prop.Images).IsRequired(false).HasConversion(converter);
        }
    }
}
