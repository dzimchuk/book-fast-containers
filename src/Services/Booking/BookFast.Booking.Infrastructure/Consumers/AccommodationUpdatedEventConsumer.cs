using BookFast.Booking.Application.Accommodations.UpsertAccommodation;
using BookFast.PropertyManagement.Integration;
using MassTransit;
using MediatR;

namespace BookFast.Booking.Infrastructure.Consumers
{
    internal class AccommodationUpdatedEventConsumer(ISender sender) : IConsumer<AccommodationUpdatedEvent>
    {
        public Task Consume(ConsumeContext<AccommodationUpdatedEvent> context)
        {
            var message = context.Message;

            var command = new UpdateAccommodationCommand
            {
                AccommodationId = message.AccommodationId,
                PropertyId = message.PropertyId,
                TenantId = message.TenantId,
                Quantity = message.Quantity,
                PriceRange = message.PriceRange,
                OccurredAt = message.OccurredAt
            };

            return sender.Send(command, context.CancellationToken);
        }
    }
}
