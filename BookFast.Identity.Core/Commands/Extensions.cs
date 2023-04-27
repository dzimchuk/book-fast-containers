using Microsoft.AspNetCore.Identity;

namespace BookFast.Identity.Core.Commands
{
    internal static class Extensions
    {
        public static void ThrowIfNotSucceeded(this IdentityResult identityResult)
        {
            if (!identityResult.Succeeded)
            {
                throw new BusinessException(identityResult.Errors.Select(error => new BusinessError(error.Code, error.Description)));
            }
        }
    }
}
