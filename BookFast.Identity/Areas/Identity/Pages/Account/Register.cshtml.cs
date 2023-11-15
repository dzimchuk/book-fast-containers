// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Models;
using BookFast.Identity.Services;
using BookFast.Integration;
using BookFast.Integration.Models.Identity;
using BookFast.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

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
        private readonly TransactionHelper transactionHelper;

        private readonly IDbContext dbContext;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IDbContext dbContext, 
            IMailNotificationQueue notificationQueue, 
            TransactionHelper transactionHelper)
        {
            this.userManager = userManager;
            this.userStore = userStore;
            emailStore = GetEmailStore();
            this.signInManager = signInManager;
            this.logger = logger;
            this.dbContext = dbContext;
            this.notificationQueue = notificationQueue;
            this.transactionHelper = transactionHelper;
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

            [Required]
            [DataType(DataType.Text)]
            [StringLength(256, ErrorMessage = "{0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Display(Name = "Tenant name")]
            public string TenantName { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        private async Task<(IdentityResult, User)> CreateTenantAdminAsync()
        {
            var tenant = new Tenant { Id = Guid.NewGuid().ToString().ToLowerInvariant(), Name = Input.TenantName };
            var user = new User();

            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync();

            user.TenantId = tenant.Id;

            await userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            var result = await userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                return (result, null);
            }

            result = await userManager.AddToRoleAsync(user, Roles.TenantAdmin);

            if (!result.Succeeded)
            {
                return (result, null);
            }

            return (result, user);
        }

        private async Task<(IdentityResult, User)> RegisterUserAndSendEmailAsync(string returnUrl)
        {
            using (var scope = transactionHelper.StartTransaction())
            {
                var (result, user) = await CreateTenantAdminAsync();

                if (result.Succeeded)
                {
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

                        await notificationQueue.EnqueueMessageAsync(message);
                    }

                    logger.LogInformation("User created a new account with password.");

                    scope.Complete();
                }

                return (result, user);
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var (result, user) = await RegisterUserAndSendEmailAsync(returnUrl);

                if (result.Succeeded)
                {
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
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
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
