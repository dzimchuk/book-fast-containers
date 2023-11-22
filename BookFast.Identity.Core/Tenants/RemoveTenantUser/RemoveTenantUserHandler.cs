using Microsoft.AspNetCore.Identity;

namespace BookFast.Identity.Core.Tenants.RemoveTenantUser
{
    public class RemoveTenantUserHandler : IRequestHandler<RemoveTenantUserCommand>
    {
        private readonly UserManager<User> userManager;
        private readonly ISecurityContext securityContext;

        public RemoveTenantUserHandler(UserManager<User> userManager, ISecurityContext securityContext)
        {
            this.userManager = userManager;
            this.securityContext = securityContext;
        }

        public async Task Handle(RemoveTenantUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null || user.TenantId != securityContext.GetCurrentTenant())
            {
                throw new NotFoundException("Specified user was not found.");
            }

            if (user.Id == securityContext.GetCurrentUser())
            {
                throw new BusinessException(ErrorCodes.SelfRemoval, "User cannot remove herself.");
            }

            var result = await userManager.DeleteAsync(user);

            result.ThrowIfNotSucceeded();
        }
    }
}
