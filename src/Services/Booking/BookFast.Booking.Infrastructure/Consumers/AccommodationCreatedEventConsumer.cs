using BookFast.Booking.Application.Accommodations.UpsertAccommodation;
using BookFast.PropertyManagement.Integration;
using MassTransit;
using MediatR;

namespace BookFast.Booking.Infrastructure.Consumers
{
    internal class AccommodationCreatedEventConsumer(ISender sender) : IConsumer<AccommodationCreatedEvent>
    {
        public Task Consume(ConsumeContext<AccommodationCreatedEvent> context)
        {
            var message = context.Message;

            var command = new CreateAccommodationCommand
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
