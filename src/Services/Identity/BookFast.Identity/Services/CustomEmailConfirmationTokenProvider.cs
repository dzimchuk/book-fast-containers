using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BookFast.Identity.Services
{
    internal class CustomEmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
        where TUser : class
    {
        public CustomEmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
                                             IOptions<CustomPasswordResetTokenProviderOptions> options,
                                             ILogger<DataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {

        }
    }

    internal class CustomEmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public CustomEmailConfirmationTokenProviderOptions()
        {
            Name = "EmailConfirmationTokenProvider";
            TokenLifespan = TimeSpan.FromDays(1);
        }
    }
}
