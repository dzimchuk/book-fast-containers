using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace BookFast.Identity.Core.Commands.ChangeRole
{
    public class ChangeRoleHandler : IRequestHandler<ChangeRoleCommand>
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

        public async Task Handle(ChangeRoleCommand request, CancellationToken cancellationToken)
        {
            using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                    TransactionScopeAsyncFlowOption.Enabled))
            {
                if (await roleManager.FindByNameAsync(request.Role) == null)
                {
                    throw new UnsupportedRoleException();
                }

                var user = await userManager.FindByIdAsync(request.UserId);
                if (user == null || user.TenantId != securityContext.GetCurrentTenant())
                {
                    throw new NotFoundException("Specified user was not found.");
                }

                if (user.Id == securityContext.GetCurrentUser())
                {
                    throw new BusinessException(ErrorCodes.SelfRoleChange, "User cannot change her role.");
                }

                var roles = await userManager.GetRolesAsync(user);

                var result = await userManager.RemoveFromRolesAsync(user, roles);

                result.ThrowIfNotSucceeded();

                result = await userManager.AddToRoleAsync(user, request.Role);

                result.ThrowIfNotSucceeded();

                scope.Complete();
            }
        }
    }
}
