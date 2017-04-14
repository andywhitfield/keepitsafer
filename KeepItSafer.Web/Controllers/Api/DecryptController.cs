using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Mvc;

namespace KeepItSafer.Web.Controllers.Api
{
    [Route("api/[controller]")]
    public class DecryptController : Controller
    {
        [HttpPost]
        public IActionResult Decrypt([FromForm] EncryptDecryptInfo info)
        {
            if (info.Group == "group-2" && info.Entry == "group-2-item-1")
            {
                if (string.IsNullOrWhiteSpace(info.MasterPassword))
                {
                    return new ObjectResult(new {
                        Decrypted = false,
                        Reason = DecryptFailReason.NeedMasterPassword,
                        Group = info.Group,
                        Entry = info.Entry
                    });
                }
                else if (info.MasterPassword == "wrong")
                {
                    return new ObjectResult(new {
                        Decrypted = false,
                        Reason = DecryptFailReason.FailedToDecrypt,
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