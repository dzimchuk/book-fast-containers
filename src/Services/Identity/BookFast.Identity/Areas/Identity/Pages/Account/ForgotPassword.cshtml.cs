// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using BookFast.Common.Application.Integration;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Email;
using BookFast.Identity.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

namespace BookFast.Identity.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly IMailNotificationQueue notificationQueue;
        private readonly IDbContext dbContext;

        public ForgotPasswordModel(UserManager<User> userManager,
                                   IMailNotificationQueue notificationQueue,
                                   IDbContext dbContext)
        {
            this.userManager = userManager;
            this.notificationQueue = notificationQueue;
            this.dbContext = dbContext;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        private async Task GenerateTokenAndSendEmailAsync(User user)
        {
            await dbContext.ExecuteInTransactionAsync(async ct =>
            {
                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var email = WebUtility.UrlEncode(Input.Email);

                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, email },
                    protocol: Request.Scheme);

                var message = new MailMessage<ResetPassword>
                {
                    To = new[] { Input.Email },
                    Subject = "Reset Password",
                    Model = new ResetPassword(callbackUrl)
                };

                await notificationQueue.EnqueueMessageAsync(message, ct);
            }, HttpContext.RequestAborted);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                await GenerateTokenAndSendEmailAsync(user);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
