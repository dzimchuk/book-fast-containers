using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BookFast.Identity.Services
{
    internal class CustomPasswordResetTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
        where TUser : class
    {
        public CustomPasswordResetTokenProvider(IDataProtectionProvider dataProtectionProvider,
                                             IOptions<CustomPasswordResetTokenProviderOptions> options,
                                             ILogger<DataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {

        }
    }

    internal class CustomPasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public CustomPasswordResetTokenProviderOptions()
        {
            Name = "PasswordResetTokenProvider";
            TokenLifespan = TimeSpan.FromHours(1);
        }
    }
}
