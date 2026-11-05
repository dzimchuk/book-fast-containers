using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.Identity.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace BookFast.Identity.Core.TenantUsers.ChangeRole
{
    public class ChangeRoleHandler : ICommandHandler<ChangeRoleCommand>
    {
        private readonly IDbContext dbContext;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;
        private readonly ISecurityContext securityContext;

        public ChangeRoleHandler(IDbContext dbContext,
                                 UserManager<User> userManager,
                                 RoleManager<Role> roleManager,
                                 ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.securityContext = securityContext;
        }

        public async Task<Result> Handle(ChangeRoleCommand request, CancellationToken cancellationToken)
        {
            if (await roleManager.FindByNameAsync(request.Role.ToLowerInvariant()) == null)
            {
                return Result.Failure<string>(ErrorCodes.UnsupportedRole);
            }

            return await dbContext.ExecuteInTransactionAsync(async ct =>
            {
                var user = await userManager.FindByIdAsync(request.UserId);
                if (user == null || user.TenantId != securityContext.GetCurrentTenant())
                {
                    return Result.Failure<string>(ErrorCodes.UserNotFound);
                }

                if (user.Id == securityContext.GetCurrentUser())
                {
                    return Result.Failure<string>(ErrorCodes.SelfRoleChange);
                }

                var roles = await userManager.GetRolesAsync(user);

                var result = await userManager.RemoveFromRolesAsync(user, roles);
                if (!result.Succeeded)
                {
                    return Result.Failure<string>(new ErrorCollection([.. result.Errors.Select(e => Error.Problem(e.Code, e.Description))]));
                }

                result = await userManager.AddToRoleAsync(user, request.Role.ToLowerInvariant());
                if (!result.Succeeded)
                {
                    return Result.Failure<string>(new ErrorCollection([.. result.Errors.Select(e => Error.Problem(e.Code, e.Description))]));
                }

                return Result.Success();
            }, cancellationToken);
        }
    }
}
