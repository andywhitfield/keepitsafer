using System.Linq;
using System.Threading.Tasks;
using KeepItSafer.Crypto;
using KeepItSafer.Web.Data;
using KeepItSafer.Web.Models;
using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KeepItSafer.Web.Controllers.Api
{
    [Route("api/[controller]")]
    public class DecryptController : Controller
    {
        private readonly IUserAccountRepository userAccountRepository;
        private readonly ILogger<DecryptController> logger;

        public DecryptController(IUserAccountRepository userAccountRepository, ILogger<DecryptController> logger)
        {
            this.userAccountRepository = userAccountRepository;
            this.logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Decrypt([FromForm] EncryptDecryptInfo info)
        {
            logger.LogDebug("Received decrypt info: {0}", info);
            if (!ModelState.IsValid)
            {
                return new ObjectResult(new {
                    Decrypted = false,
                    Reason = ActionFailReason.InvalidInput,
                    Group = info.Group,
                    Entry = info.Entry
                });
            }

            var masterPassword = info.MasterPassword;
            if (string.IsNullOrWhiteSpace(masterPassword))
            {
                masterPassword = this.RememberedMasterPassword();
            }
            else if (info.RememberMasterPassword)
            {
                this.RememberMasterPassword(info.MasterPassword);
            }

            if (string.IsNullOrWhiteSpace(masterPassword))
            {
                return new ObjectResult(new {
                    Decrypted = false,
                    Reason = ActionFailReason.NeedMasterPassword,
                    Group = info.Group,
                    Entry = info.Entry
                });
            }

            using (var secure = new Secure())
            {
                var userAccount = await userAccountRepository.GetUserAccountAsync(User);
                var passwordDb = userAccount.GetPasswordDb();

                if (!secure.ValidateHash(masterPassword, passwordDb.MasterPassword))
                {
                    return new ObjectResult(new {
                        Added = false,
                        Reason = ActionFailReason.MasterPasswordInvalid,
                        Group = info.Group,
                        Entry = info.Entry
                    });
                }

                var group = passwordDb.PasswordGroups.FirstOrDefault(pg => pg.GroupName == info.Group);
                if (group == null)
                {
                    return new ObjectResult(new {
                        Decrypted = false,
                        Reason = ActionFailReason.InvalidInput,
                        Group = info.Group,
                        Entry = info.Entry
                    });
                }
                var entry = group.PasswordEntries.FirstOrDefault(pe => pe.Name == info.Entry);
                if (entry == null)
                {
                    return new ObjectResult(new {
                        Decrypted = false,
                        Reason = ActionFailReason.InvalidInput,
                        Group = info.Group,
                        Entry = info.Entry
                    });
                }
                if (!entry.IsValueEncrypted)
                {
                    return new ObjectResult(new {
                        Decrypted = true,
                        DecryptedValue = entry.Value,
                        Group = info.Group,
                        Entry = info.Entry
                    });
                }
                
                return new ObjectResult(new {
                    Decrypted = true,
                    DecryptedValue = secure.Decrypt(masterPassword, passwordDb.IV, entry.Salt, entry.Value),
                    Group = info.Group,
                    Entry = info.Entry
                });
            }
        }
    }
}