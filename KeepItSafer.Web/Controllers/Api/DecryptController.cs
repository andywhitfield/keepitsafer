using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Mvc;

namespace KeepItSafer.Web.Controllers.Api
{
    [Route("api/[controller]")]
    public class DecryptController : Controller
    {
        [HttpPost]
        public IActionResult Decrypt([FromForm] DecryptItem item)
        {
            if (item.Group == "group-2" && item.Entry == "group-2-item-1")
            {
                if (string.IsNullOrWhiteSpace(item.MasterPassword))
                {
                    return new ObjectResult(new {
                        Decrypted = false,
                        Reason = DecryptFailReason.NeedMasterPassword
                    });
                }
                else if (item.MasterPassword == "wrong")
                {
                    return new ObjectResult(new {
                        Decrypted = false,
                        Reason = DecryptFailReason.FailedToDecrypt
                    });
                }
                else
                {
                    return new ObjectResult(new {
                        Decrypted = true,
                        DecryptedValue = "decrypted using master password!"
                    });
                }
            }

            return new ObjectResult(new {
                Decrypted = true,
                DecryptedValue = "hello world"
            });
        }
    }
}