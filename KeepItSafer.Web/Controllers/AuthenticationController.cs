using System.Linq;
using KeepItSafer.Web.Data;
using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
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
        public IActionResult SignIn() => View("SignIn", HttpContext.Authentication.GetAuthenticationSchemes().FirstOrDefault(a => a.AuthenticationScheme == "smallid"));

        [HttpPost("~/signin")]
        public IActionResult SignIn([FromForm] string provider)
        {
            // Note: the "provider" parameter corresponds to the external
            // authentication provider choosen by the user agent.
            if (string.IsNullOrWhiteSpace(provider))
            {
                return BadRequest();
            }

            // Instruct the middleware corresponding to the requested external identity
            // provider to redirect the user agent to its own authorization endpoint.
            // Note: the authenticationScheme parameter must match the value configured in Startup.cs
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, provider);
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
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}