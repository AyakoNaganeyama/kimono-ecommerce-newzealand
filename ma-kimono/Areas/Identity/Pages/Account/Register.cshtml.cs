// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ma_kimono.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ma_kimono.Models.DB;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

/*================================================ 
 * Google chrome is strick with cookes and cores so manually setting of cookies and google created users needs to be done
 * Authentication logic adapted from external sources.

 * ================================================
 **/


namespace ma_kimono.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly Fs2024s1Project01ProjectContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            // Include context here becasue need to insert data to customer table too 
            Fs2024s1Project01ProjectContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime Birthday { get; set; }
            //27/05/ This goes to customer table Ayako and Multi
            public string CustomerMobileNumber { get; set; }
            public string CustomerAddress { get; set; }
            public string CustomerPaymentMethod { get; set; }
            public string UserId { get; set; }
            public bool IsSubscribed { get; set; }

        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        // handle extern login api handshake, code refrance can be found at top of file
        public IActionResult OnPostExternalLogin(string provider, string returnUrl = null)
        {
            // set redirect instructions
            var redirectUrl = Url.Page("./Register", pageHandler: "ExternalLoginCallback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }

        // handle normal email and password sign up
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.Birthday = Input.Birthday;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //27/05/ Ayako and Multi create new customer  
                    var newCustomer = new Customer
                    {
                        CustomerMobileNumber = Input.CustomerMobileNumber,
                        CustomerAddress = Input.CustomerAddress,
                        CustomerPaymentMethod = Input.CustomerPaymentMethod,
                        UserId = userId, // this is retrieved from the newly created user
                        IsSubscribed = Input.IsSubscribed
                    };

                    _context.Customers.Add(newCustomer);
                    await _context.SaveChangesAsync();

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // if something goes wrong just redirect back 
            return LocalRedirect(returnUrl);
        }

        /*================================================
         * Google chrome is strick with cookes and cores so manually setting of cookies and google created users needs to be done,
         *Authentication logic adapted from external sources.
         * ================================================
         **/
        public async Task<IActionResult> OnGetExternalLoginCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            var googleUser = await _signInManager.GetExternalLoginInfoAsync();

            var email = googleUser.Principal.FindFirstValue(ClaimTypes.Email);
            var firstName = googleUser.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = googleUser.Principal.FindFirstValue(ClaimTypes.Surname);

            // double got google user data
            if (email != null && firstName != null && lastName != null)
            {
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                };

                var result = await _userManager.CreateAsync(user);
                await _context.SaveChangesAsync();

                if (result.Succeeded)
                {

                    // cant access google user address, requires more config in api
                    var newCustomer = new Customer
                    {
                        CustomerMobileNumber = "",
                        CustomerAddress = "",
                        CustomerPaymentMethod = "",
                        UserId = user.Id, // This is retrieved from the newly created user
                        IsSubscribed = false,
                    };

                    _context.Customers.Add(newCustomer);
                    await _context.SaveChangesAsync(); // make sure changes are saved

                    // login user
                    result = await _userManager.AddLoginAsync(user, googleUser);
                    if (result.Succeeded)
                    {
                        // manually get tokens as cookies are strict in google chrome
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { userId = user.Id, token = token },
                            protocol: Request.Scheme);

                        // Send confirmation email
                        await _emailSender.SendEmailAsync(email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>clicking here</a>.");

                        // Redirect to RegisterConfirmation page
                        return RedirectToPage("RegisterConfirmation", new { email = email, returnUrl = returnUrl });
                    }
                }
            }

            _logger.LogError("Failed to create user or add external login.");
            return RedirectToPage("./Register", new { ReturnUrl = returnUrl });
        }


        private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}
