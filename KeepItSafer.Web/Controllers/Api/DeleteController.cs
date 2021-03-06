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
    public class DeleteController : Controller
    {
        private readonly IUserAccountRepository userAccountRepository;
        private readonly ILogger<DeleteController> logger;

        public DeleteController(IUserAccountRepository userAccountRepository, ILogger<DeleteController> logger)
        {
            this.userAccountRepository = userAccountRepository;
            this.logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Delete([FromForm] EncryptDecryptInfo info)
        {
            logger.LogDebug("Received delete info: {0}", info);
            if (!ModelState.IsValid)
            {
                return new ObjectResult(new {
                    Deleted = false,
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
                    Deleted = false,
                    Reason = ActionFailReason.NeedMasterPassword,
                    Group = info.Group,
                    Entry = info.Entry
                });
            }

            var userAccount = await userAccountRepository.GetUserAccountAsync(User);
            var passwordDb = userAccount.GetPasswordDb();

            using (var secure = new Secure())
            {
                if (!secure.ValidateHash(masterPassword, passwordDb.MasterPassword))
                {
                    return new ObjectResult(new {
                        Deleted = false,
                        Reason = ActionFailReason.MasterPasswordInvalid,
                        Group = info.Group,
                        Entry = info.Entry
                    });
                }
            }

            var group = passwordDb.PasswordGroups.FirstOrDefault(pg => pg.GroupName == info.Group);
            if (group == null)
            {
                return new ObjectResult(new {
                    Deleted = false,
                    Reason = ActionFailReason.InvalidInput,
                    Group = info.Group,
                    Entry = info.Entry
                });
            }
            var entry = group.PasswordEntries.FirstOrDefault(pe => pe.Name == info.Entry);
            if (entry == null)
            {
                return new ObjectResult(new {
                    Deleted = false,
                    Reason = ActionFailReason.InvalidInput,
                    Group = info.Group,
                    Entry = info.Entry
                });
            }

            group.PasswordEntries.Remove(entry);
            if (group.PasswordEntries.Count == 0)
            {
                passwordDb.PasswordGroups.Remove(group);
            }
            
            userAccount.SetPasswordDb(passwordDb);
            await userAccountRepository.SaveUserAccountAsync(userAccount);

            return new ObjectResult(new {
                Deleted = true,
                Group = info.Group,
                Entry = info.Entry
            });
        }
    }
}