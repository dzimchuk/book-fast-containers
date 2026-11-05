using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.PropertyManagement.Integration;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.Accommodations.DeleteAccommodation
{
    public class DeleteAccommodationHandler : ICommandHandler<DeleteAccommodationCommand>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;
        private readonly IIntegrationEventPublisher eventPublisher;

        public DeleteAccommodationHandler(IDbContext dbContext,
                                          ISecurityContext securityContext,
                                          IIntegrationEventPublisher eventPublisher)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
            this.eventPublisher = eventPublisher;
        }

        public async Task<Result> Handle(DeleteAccommodationCommand request, CancellationToken cancellationToken)
        {
            return await dbContext.ExecuteInTransactionAsync(async ct =>
            {
                var accommodation = await dbContext.Accommodations.FirstOrDefaultAsync(
                    a => a.Id == request.AccommodationId && a.TenantId == securityContext.GetCurrentTenant(),
                    ct);
                if (accommodation == null)
                {
                    return ErrorCodes.AccommodationNotFound(request.AccommodationId);
                }

                dbContext.Accommodations.Remove(accommodation);

                await dbContext.SaveChangesAsync(ct);

                await eventPublisher.PublishAsync(new AccommodationDeletedEvent
                {
                    TenantId = securityContext.GetCurrentTenant(),
                    AccommodationId = accommodation.Id,
                    PropertyId = accommodation.PropertyId
                }, ct);

                return Result.Success();
            }, cancellationToken);
        }
    }
}
