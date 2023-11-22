// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using BookFast.Identity.Core.Models;
using BookFast.Identity.Services;
using BookFast.Integration;
using BookFast.Integration.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BookFast.Identity.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly IMailNotificationQueue notificationQueue;
        private readonly TransactionHelper transactionHelper;

        public ResendEmailConfirmationModel(UserManager<User> userManager,
                                            IMailNotificationQueue notificationQueue,
                                            TransactionHelper transactionHelper)
        {
            this.userManager = userManager;
            this.notificationQueue = notificationQueue;
            this.transactionHelper = transactionHelper;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        public bool ShowMessageSent { get; set; }

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

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await userManager.FindByEmailAsync(Input.Email);

            if (user != null)
            {
                using (var scope = transactionHelper.StartTransaction())
                {
                    var userId = await userManager.GetUserIdAsync(user);
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId, code },
                        protocol: Request.Scheme);

                    var message = new MailMessage<ConfirmEmail>
                    {
                        To = new[] { Input.Email },
                        Subject = "Registration confirmation",
                        Model = new ConfirmEmail(callbackUrl)
                    };

                    await notificationQueue.EnqueueMessageAsync(message);

                    scope.Complete();
                } 
            }

            ShowMessageSent = true;

            return Page();
        }
    }
}
