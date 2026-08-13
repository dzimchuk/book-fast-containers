using BookFast.Booking.Domain;
using BookFast.Booking.Integration;
using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Domain;
using BookFast.Common.SeedWork;

namespace BookFast.Booking.Application.Accommodations.UpsertAccommodation
{
    public class UpsertAccommodationCommandHandler(IDbContext dbContext, IIntegrationEventPublisher eventPublisher)
        : ICommandHandler<CreateAccommodationCommand>,
          ICommandHandler<UpdateAccommodationCommand>
    {
        public Task<Result> Handle(CreateAccommodationCommand request, CancellationToken cancellationToken)
        {
            return UpsertAsync(request.AccommodationId,
                               request.TenantId,
                               request.PropertyId,
                               request.Quantity,
                               SeedRate(request.PriceRange),
                               forceSeed: true,
                               request.OccurredAt,
                               cancellationToken);
        }

        public Task<Result> Handle(UpdateAccommodationCommand request, CancellationToken cancellationToken)
        {
            return UpsertAsync(request.AccommodationId,
                               request.TenantId,
                               request.PropertyId,
                               request.Quantity,
                               SeedRate(request.PriceRange),
                               forceSeed: false,
                               request.OccurredAt,
                               cancellationToken);
        }

        private static Money SeedRate(PriceRange priceRange) => priceRange?.MinPrice ?? priceRange?.MaxPrice;

        private async Task<Result> UpsertAsync(
            Guid accommodationId, string tenantId, Guid propertyId, int quantity, Money rate, bool forceSeed, DateTimeOffset occurredAt, CancellationToken cancellationToken)
        {
            return await dbContext.ExecuteInTransactionAsync(async ct =>
            {
                var accommodation = await dbContext.Accommodations.FindAsync([accommodationId], ct);

                if (accommodation is null)
                {
                    accommodation = Accommodation.NewAccommodation(accommodationId, tenantId, propertyId);
                    dbContext.Accommodations.Add(accommodation);
                }

                var changed = forceSeed
                    ? accommodation.ApplyCreated(quantity, rate, occurredAt)
                    : accommodation.ApplyUpdated(quantity, rate, occurredAt);

                await dbContext.SaveChangesAsync(ct);

                if (changed)
                {
                    await PublishBookableChangedAsync(accommodation, ct);
                }

                return Result.Success();
            }, cancellationToken);
        }

        private Task PublishBookableChangedAsync(Accommodation accommodation, CancellationToken cancellationToken) =>
            eventPublisher.PublishAsync(new AccommodationBookableChanged
            {
                OccurredAt = accommodation.OccurredAt,
                AccommodationId = accommodation.Id,
                Bookable = accommodation.Bookable,
                Rate = accommodation.Rate,
            }, cancellationToken);
    }
}
