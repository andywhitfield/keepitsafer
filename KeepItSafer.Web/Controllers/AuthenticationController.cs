using System.Linq;
using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace KeepItSafer.Web.Controllers
{
    public class AuthenticationController : Controller
    {
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
        public IActionResult CreateMasterPassword()
        {
            if (HttpContext.Session.Keys.Contains("newuser"))
            {
                return Redirect("~/");
            }

            HttpContext.Session.Set("newuser", new byte[0]);
            return View("CreateMasterPassword");
        }

        [HttpPost("~/newuser")]
        public IActionResult CreateMasterPassword([FromForm] NewMasterPasswordInfo newDetails)
        {
            // TODO: validate passwords are the same;
            //       return a message back on the page in case of error
            if (!ModelState.IsValid)
            {
                return CreateMasterPassword();
            }

            HttpContext.Session.Set("newuser", new byte[0]);
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