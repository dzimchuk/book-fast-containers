using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.Identity.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace BookFast.Identity.Core.TenantUsers.RemoveTenantUser
{
    public class RemoveTenantUserHandler : ICommandHandler<RemoveTenantUserCommand>
    {
        private readonly UserManager<User> userManager;
        private readonly ISecurityContext securityContext;

        public RemoveTenantUserHandler(UserManager<User> userManager, ISecurityContext securityContext)
        {
            this.userManager = userManager;
            this.securityContext = securityContext;
        }

        public async Task<Result> Handle(RemoveTenantUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null || user.TenantId != securityContext.GetCurrentTenant())
            {
                return Result.Failure<string>(ErrorCodes.UserNotFound);
            }

            if (user.Id == securityContext.GetCurrentUser())
            {
                return Result.Failure<string>(ErrorCodes.SelfRemoval);
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return Result.Failure<string>(new ErrorCollection([.. result.Errors.Select(e => Error.Problem(e.Code, e.Description))]));
            }

            return Result.Success();
        }
    }
}
