using BookFast.Common.Application.Integration;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Email;
using BookFast.Identity.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace BookFast.Identity.Services
{
    internal class EmailConfirmationSender(UserManager<User> userManager,
                                           LinkGenerator linkGenerator,
                                           IHttpContextAccessor httpContextAccessor,
                                           IMailNotificationQueue notificationQueue)
        : IEmailConfirmationSender
    {
        public async Task SendAsync(User user)
        {
            var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = linkGenerator.GetUriByPage(
                            httpContextAccessor.HttpContext,
                            page: "/Account/ConfirmEmail",
                            handler: null,
                            values: new { area = "Identity", userId, code });

            var message = new MailMessage<ConfirmEmail>
            {
                To = [user.Email],
                Subject = "Registration confirmation",
                Model = new ConfirmEmail(callbackUrl)
            };

            await notificationQueue.EnqueueMessageAsync(message);
        }
    }
}
