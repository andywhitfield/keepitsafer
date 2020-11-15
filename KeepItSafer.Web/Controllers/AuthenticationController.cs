using System;
using System.Threading.Tasks;
using Dropbox.Api;
using KeepItSafer.Web.Data;
using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KeepItSafer.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserAccountRepository userAccountRepository;
        private readonly ILogger<AuthenticationController> logger;
        private readonly string dropboxAppKey;
        private readonly string dropboxAppSecret;

        public AuthenticationController(IUserAccountRepository userAccountRepository,
            ILogger<AuthenticationController> logger,
            IConfiguration configuration)
        {
            this.userAccountRepository = userAccountRepository;
            this.logger = logger;
            this.dropboxAppKey = configuration["Dropbox:KeepItSaferAppKey"];
            this.dropboxAppSecret = configuration["Dropbox:KeepItSaferAppSecret"];
        }


        [HttpGet("~/signin")]
        public IActionResult Signin() => View("SignIn");

        [HttpPost("~/signin")]
        public IActionResult SigninChallenge()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("~/newuser")]
        [Authorize]
        public async Task<ActionResult> CreateMasterPassword(string error = null)
        {
            if (await userAccountRepository.HasMasterPasswordAsync(User))
            {
                logger.LogDebug("User already has a master password, redirecting to the home page.");
                return Redirect("~/");
            }
            
            return View("CreateMasterPassword", error);
        }

        [HttpPost("~/newuser")]
        [Authorize]
        public async Task<ActionResult> CreateMasterPassword([FromForm] NewMasterPasswordInfo newDetails)
        {
            if (!ModelState.IsValid)
            {
                return await CreateMasterPassword("Please enter a master password and the confirm the same value below.");
            }

            await userAccountRepository.CreateNewUserAsync(User, newDetails.NewMasterPassword);
            return Redirect("~/");
        }

        [HttpGet("~/signout"), HttpPost("~/signout")]
        public IActionResult Signout()
        {
            HttpContext.Session.Clear();
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder();
                uriBuilder.Scheme = Request.Scheme;
                uriBuilder.Host = Request.Host.Host;
                if (Request.Host.Port.HasValue && Request.Host.Port != 443 && Request.Host.Port != 80)
                    uriBuilder.Port = Request.Host.Port.Value;
                uriBuilder.Path = "dropbox-authentication";
                return uriBuilder.Uri;
            }
        }

        [HttpGet("~/dropbox-connect")]
        [Authorize]
        public IActionResult ConnectToDropbox()
        {
            var dropboxRedirect = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, dropboxAppKey, RedirectUri);
            logger.LogInformation($"Getting user token from Dropbox: {dropboxRedirect} (redirect={RedirectUri})");
            return Redirect(dropboxRedirect.ToString());
        }

        [HttpGet("~/dropbox-authentication")]
        [Authorize]
        public async Task<ActionResult> DropboxAuthentication(string code, string state)
        {
            var response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(code, dropboxAppKey, dropboxAppSecret, RedirectUri.ToString());
            logger.LogInformation($"Got user token from Dropbox: {response.AccessToken}");

            var userAccount = await userAccountRepository.GetUserAccountAsync(User);
            userAccount.DropboxToken = response.AccessToken;
            await userAccountRepository.SaveUserAccountAsync(userAccount);

            return Redirect("~/");
        }

        [HttpGet("~/dropbox-disconnect")]
        [Authorize]
        public async Task<ActionResult> DisconnectFromDropbox()
        {
            var userAccount = await userAccountRepository.GetUserAccountAsync(User);
            userAccount.DropboxToken = null;
            await userAccountRepository.SaveUserAccountAsync(userAccount);
            return Redirect("~/");
        }
    }
}