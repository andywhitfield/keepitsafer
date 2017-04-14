using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KeepItSafer.Web.Controllers.Api
{
    [Route("api/[controller]")]
    public class DecryptController : Controller
    {
        private readonly ILogger logger;

        public DecryptController(ILogger<DecryptController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        public IActionResult Decrypt([FromForm] EncryptDecryptInfo info)
        {
            logger.LogDebug("Received decrypt info: {0}", info);
            if (info.Group == "group-2" && info.Entry == "group-2-item-1")
            {
                if (string.IsNullOrWhiteSpace(info.MasterPassword))
                {
                    return new ObjectResult(new {
                        Decrypted = false,
                        Reason = ActionFailReason.NeedMasterPassword,
                        Group = info.Group,
                        Entry = info.Entry
                    });
                }
                else if (info.MasterPassword == "wrong")
                {
                    return new ObjectResult(new {
                        Decrypted = false,
                        Reason = ActionFailReason.MasterPasswordInvalid,
                        Group = info.Group,
                        Entry = info.Entry
                    });
                }
                else if (info.MasterPassword == "error")
                {
                    return new ObjectResult(new {
                        Decrypted = false,
                        Reason = -1,
                        Group = info.Group,
                        Entry = info.Entry
                    });
                }
                else
                {
                    return new ObjectResult(new {
                        Decrypted = true,
                        DecryptedValue = "decrypted using master password!",
                        Group = info.Group,
                        Entry = info.Entry
                    });
                }
            }

            return new ObjectResult(new {
                Decrypted = true,
                DecryptedValue = "hello world",
                Group = info.Group,
                Entry = info.Entry
            });
        }
    }
}