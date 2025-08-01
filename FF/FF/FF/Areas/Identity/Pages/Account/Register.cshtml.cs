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
using FF.Models; // تأكد من وجود هذا السطر لـ ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace FF.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager; // إضافة RoleManager هنا

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager) // حقن RoleManager في الدالة الإنشائية
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager; // تهيئة RoleManager
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "الاسم الكامل مطلوب.")]
            [StringLength(100, ErrorMessage = "الاسم الكامل يجب أن يكون بين {2} و {1} حرفاً.", MinimumLength = 2)]
            [Display(Name = "الاسم الكامل")]
            public string FullName { get; set; } // خاصية الاسم الكامل

            [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
            [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
            [Display(Name = "البريد الإلكتروني")]
            public string Email { get; set; }

            [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
            [StringLength(100, ErrorMessage = "كلمة المرور يجب أن تكون بين {2} و {1} حرفاً.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "كلمة المرور")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "تأكيد كلمة المرور")]
            [Compare("Password", ErrorMessage = "كلمة المرور وتأكيد كلمة المرور غير متطابقين.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // تعيين قيمة الاسم الكامل من النموذج قبل حفظ المستخدم
                user.FullName = Input.FullName;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // =================================================================
                    // === هذا هو الجزء الحاسم لتعيين دور "Customer" للمستخدم الجديد ===
                    // =================================================================
                    var defaultRole = "Customer";
                    if (!await _roleManager.RoleExistsAsync(defaultRole))
                    {
                        // إذا لم يكن الدور موجوداً (وهو أمر غير متوقع بعد الـ seeding في Program.cs، ولكن للتأكيد)
                        await _roleManager.CreateAsync(new IdentityRole(defaultRole));
                    }
                    await _userManager.AddToRoleAsync(user, defaultRole);
                    _logger.LogInformation($"User {user.Email} assigned to role {defaultRole}.");
                    // =================================================================

                    // تعطيل تأكيد البريد الإلكتروني مؤقتاً لتسهيل الاختبار
                    // يمكنك إعادة تفعيل هذا الجزء لاحقاً إذا كنت بحاجة إلى تأكيد البريد الإلكتروني
                    // var userId = await _userManager.GetUserIdAsync(user);
                    // var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    // code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    // var callbackUrl = Url.Page(
                    //     "/Account/ConfirmEmail",
                    //     pageHandler: null,
                    //     values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                    //     protocol: Request.Scheme);
                    // await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //     $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        // =================================================================
                        // === إعادة توجيه المستخدم الجديد (العميل) إلى صفحة إدارة الطلبات ===
                        // =================================================================
                        return RedirectToAction("Index", "Orders"); // التوجيه إلى صفحة الطلبات
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

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
