using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KeepItSafer.Web.Controllers.Api
{
    [Route("api/[controller]")]
    public class AddController : Controller
    {
        private readonly ILogger logger;
        public AddController(ILogger<DecryptController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        public IActionResult Add([FromForm] EncryptDecryptInfo info)
        {
            logger.LogDebug("Received encrypt info: {0}", info);
            if (!ModelState.IsValid)
            {
                return new ObjectResult(new {
                    Added = false,
                    Reason = ActionFailReason.InvalidInput,
                    Group = info.Group,
                    Entry = info.Entry
                });
            }

            if (info.Group == "new-group" && info.Entry == "new-entry" && string.IsNullOrWhiteSpace(info.MasterPassword))
            {
                return new ObjectResult(new {
                    Added = false,
                    Reason = ActionFailReason.NeedMasterPassword,
                    Group = info.Group,
                    Entry = info.Entry
                });
            }

            return new ObjectResult(new {
                Added = true,
                Group = info.Group,
                Entry = info.Entry
            });
        }
    }
}