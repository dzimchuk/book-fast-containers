using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.Identity.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace BookFast.Identity.Core.TenantUsers.ChangeRole
{
    public class ChangeRoleHandler : ICommandHandler<ChangeRoleCommand>
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;
        private readonly ISecurityContext securityContext;

        public ChangeRoleHandler(UserManager<User> userManager,
                                 RoleManager<Role> roleManager,
                                 ISecurityContext securityContext)
        {
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

            using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                    TransactionScopeAsyncFlowOption.Enabled))
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

                scope.Complete();

                return Result.Success();
            }
        }
    }
}
