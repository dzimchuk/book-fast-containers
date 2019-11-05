using BookFast.ReliableEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookFast.Facility.Data.Configurations
{
    internal class ReliableEventConfiguration : IEntityTypeConfiguration<ReliableEvent>
    {
        public void Configure(EntityTypeBuilder<ReliableEvent> builder)
        {
            builder.ToTable("Events", "facility");

            builder.HasKey(@event => @event.Id);

            builder.Property(@event => @event.EventName).IsRequired(true).HasMaxLength(100);
            builder.Property(@event => @event.OccurredAt).IsRequired(true);
            builder.Property(@event => @event.User).IsRequired(true).HasMaxLength(50);
            builder.Property(@event => @event.Tenant).IsRequired(true).HasMaxLength(50);
            builder.Property(@event => @event.Payload).IsRequired(true);
        }
    }
}
