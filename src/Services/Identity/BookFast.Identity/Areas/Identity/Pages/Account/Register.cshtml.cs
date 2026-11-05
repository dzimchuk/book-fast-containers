// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using BookFast.Common.Application.Integration;
using BookFast.Common.SeedWork;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Email;
using BookFast.Identity.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BookFast.Identity.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly IUserStore<User> userStore;
        private readonly IUserEmailStore<User> emailStore;
        private readonly ILogger<RegisterModel> logger;
        private readonly IMailNotificationQueue notificationQueue;
        private readonly IDbContext dbContext;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IMailNotificationQueue notificationQueue, 
            IDbContext dbContext)
        {
            this.userManager = userManager;
            this.userStore = userStore;
            emailStore = GetEmailStore();
            this.signInManager = signInManager;
            this.logger = logger;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        private async Task<Result<User>> CreateNewUserAsync(CancellationToken cancellationToken)
        {
            var user = new User();

            await userStore.SetUserNameAsync(user, Input.Email, cancellationToken);
            await emailStore.SetEmailAsync(user, Input.Email, cancellationToken);
            var result = await userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                return Result.Failure<User>(
                    new ErrorCollection([.. result.Errors.Select(e => Error.Problem(e.Code, e.Description))]));
            }

            return user;
        }

        private async Task<Result<User>> RegisterUserAndSendEmailAsync(string returnUrl)
        {
            return await dbContext.ExecuteInTransactionAsync(async ct =>
            {
                var result = await CreateNewUserAsync(ct);

                if (result.IsSuccess)
                {
                    var user = result.Value;

                    if (userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        var userId = await userManager.GetUserIdAsync(user);
                        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId, code, returnUrl },
                            protocol: Request.Scheme);

                        var message = new MailMessage<ConfirmEmail>
                        {
                            To = new[] { Input.Email },
                            Subject = "Registration confirmation",
                            Model = new ConfirmEmail(callbackUrl)
                        };

                        await notificationQueue.EnqueueMessageAsync(message, ct);
                    }

                    logger.LogInformation("User created a new account with password.");
                }

                return result;
            }, HttpContext.RequestAborted);
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var result = await RegisterUserAndSendEmailAsync(returnUrl);

                if (result.IsSuccess)
                {
                    var user = result.Value;

                    if (userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }
                    else
                    {
                        await signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                AddModelStateErrors(result.Error);
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private void AddModelStateErrors(Error error)
        {
            if (error is ErrorCollection collection)
            {
                foreach (var innerError in collection.Errors)
                {
                    ModelState.AddModelError(string.Empty, innerError.Description);
                }

                return;
            }

            ModelState.AddModelError(string.Empty, error.Description);
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)userStore;
        }
    }
}
