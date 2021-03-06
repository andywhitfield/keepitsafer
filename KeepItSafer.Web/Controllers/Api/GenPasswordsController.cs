using System.Collections.Generic;
using System.Linq;
using KeepItSafer.Crypto.PasswordGenerator;
using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KeepItSafer.Web.Controllers.Api
{
    [Route("api/[controller]")]
    public class GenPasswordsController : Controller
    {
        private readonly RandomPasswordGenerator passwordGenerator;
        private readonly ILogger<GenPasswordsController> logger;

        public GenPasswordsController(RandomPasswordGenerator passwordGenerator, ILogger<GenPasswordsController> logger)
        {
            this.passwordGenerator = passwordGenerator;
            this.logger = logger;
        }

        [HttpPost]
        public IActionResult Generate([FromForm] GenPasswordsInfo info)
        {
            logger.LogDebug("Received gen password info: {0}", info);
            if (!ModelState.IsValid)
            {
                return new ObjectResult(new {
                    Passwords = new string[0],
                    Reason = ActionFailReason.InvalidInput
                });
            }

            passwordGenerator.MinimumLength = info.MinLength;
            passwordGenerator.MaximumLength = info.MaxLength;
            passwordGenerator.AllowNumbers = info.AllowNumbers;
            passwordGenerator.AllowPunctuation = info.AllowSpecialCharacters;

            return new ObjectResult(new { Passwords = GeneratePasswords().ToArray() });
        }

        private IEnumerable<string> GeneratePasswords()
        {
            var randomPassword = passwordGenerator.Generate();
            // password generator not yet ready to handle requests, return early...
            if (randomPassword == null)
            {
                yield break;
            }
            yield return randomPassword;
            for (var i = 0; i < 7; i++)
            {
                yield return passwordGenerator.Generate();
            }
        }
    }
}