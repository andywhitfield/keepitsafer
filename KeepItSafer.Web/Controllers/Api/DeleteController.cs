using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KeepItSafer.Web.Controllers.Api
{
    [Route("api/[controller]")]
    public class DeleteController
    {
        private readonly ILogger logger;

        public DeleteController(ILogger<DeleteController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        public IActionResult Delete([FromForm] EncryptDecryptInfo info)
        {
            logger.LogDebug("Received delete info: {0}", info);
            if (info.Group == "group-2" && info.Entry == "group-2-item-1")
            {
                if (string.IsNullOrWhiteSpace(info.MasterPassword))
                {
                    return new ObjectResult(new {
                        Deleted = false,
                        Reason = ActionFailReason.NeedMasterPassword,
                        Group = info.Group,
                        Entry = info.Entry
                    });
                }
                return new ObjectResult(new {
                    Deleted = true,
                    Group = info.Group,
                    Entry = info.Entry
                });
            }

            return new ObjectResult(new {
                Deleted = true,
                Group = info.Group,
                Entry = info.Entry
            });
        }
    }
}