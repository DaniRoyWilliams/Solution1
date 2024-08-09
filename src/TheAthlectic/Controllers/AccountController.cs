using TheAthletic.core.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;

namespace TheAthletic.Core.Controllers
{
    public class AccountController : SurfaceController
    {
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;
        private readonly IMemberSignInManager _memberSignInManager;
        private readonly ICoreScopeProvider _coreScopeProvider;

        public AccountController(
            IMemberManager memberManager,
            IMemberService memberService,
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IMemberSignInManager memberSignInManager,
            ICoreScopeProvider coreScopeProvider)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager;
            _memberService = memberService;
            _memberSignInManager = memberSignInManager;
            _coreScopeProvider = coreScopeProvider;
        }

        public IActionResult Index()
        {
            ViewData["IsAuthenticated"] = User.Identity.IsAuthenticated;
            return View();
        }

        // Show Register Page
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // Handle Registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleRegisterMember(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }

            var identityUser = MemberIdentityUser.CreateNew(model.Email, model.Email, "Member", true, model.Name);
            var result = await _memberManager.CreateAsync(identityUser, model.Password);

            if (result.Succeeded)
            {
                var member = _memberService.GetByKey(identityUser.Key);
                if (member != null)
                {
                    member.Name = model.Name;
                    _memberService.Save(member);
                    await _memberSignInManager.SignInAsync(identityUser, isPersistent: false);
                    return Redirect("/");
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Register", model);
        }

        // Show Login Page
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // Handle Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleLoginMember(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            try
            {
                var result = await _memberSignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return Redirect("/");
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                throw;
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("Login", model);
        }

        // Handle Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _memberSignInManager.SignOutAsync();
            return Redirect("/");
        }
    }
}
