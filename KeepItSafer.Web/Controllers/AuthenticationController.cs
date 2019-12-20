using KeepItSafer.Web.Data;
using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KeepItSafer.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserAccountRepository userAccountRepository;
        private readonly ILogger<AuthenticationController> logger;

        public AuthenticationController(IUserAccountRepository userAccountRepository,
            ILogger<AuthenticationController> logger)
        {
            this.userAccountRepository = userAccountRepository;
            this.logger = logger;
        }


        [HttpGet("~/signin")]
        public IActionResult SignIn() => View("SignIn");

        [HttpPost("~/signin")]
        public IActionResult SignInChallenge()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("~/newuser")]
        [Authorize]
        public IActionResult CreateMasterPassword(string error = null)
        {
            if (userAccountRepository.HasMasterPassword(User))
            {
                logger.LogDebug("User already has a master password, redirecting to the home page.");
                return Redirect("~/");
            }
            
            return View("CreateMasterPassword", error);
        }

        [HttpPost("~/newuser")]
        [Authorize]
        public IActionResult CreateMasterPassword([FromForm] NewMasterPasswordInfo newDetails)
        {
            if (!ModelState.IsValid)
            {
                return CreateMasterPassword("Please enter a master password and the confirm the same value below.");
            }

            userAccountRepository.CreateNewUser(User, newDetails.NewMasterPassword);
            return Redirect("~/");
        }

        [HttpGet("~/signout"), HttpPost("~/signout")]
        public IActionResult SignOut()
        {
            HttpContext.Session.Clear();
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}