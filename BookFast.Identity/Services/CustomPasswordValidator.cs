using BookFast.Identity.Core;
using BookFast.Identity.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace BookFast.Identity.Services
{
    internal class CustomPasswordValidator : IPasswordValidator<User>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string password)
        {
            var errors = new List<IdentityError>();

            CheckUsernameAsPassword(user, password, errors);

            var result = errors.Any()
                ? IdentityResult.Failed([.. errors])
                : IdentityResult.Success;

            return Task.FromResult(result);
        }

        private static void CheckUsernameAsPassword(User user, string password, List<IdentityError> errors)
        {
            if (string.Equals(user.UserName, password, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(user.Email, password, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new IdentityError
                {
                    Code = ErrorCodes.UsernameAsPassword,
                    Description = Resources.Messages.UsernameAsPassword
                });
            }
        }
    }
}
