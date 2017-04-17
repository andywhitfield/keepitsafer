using System.Linq;
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
    public class AddController : Controller
    {
        private readonly IUserAccountRepository userAccountRepository;
        private readonly ILogger<AddController> logger;

        public AddController(IUserAccountRepository userAccountRepository, ILogger<AddController> logger)
        {
            this.userAccountRepository = userAccountRepository;
            this.logger = logger;
        }

        [HttpPost]
        [Authorize]
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
                    Added = false,
                    Reason = ActionFailReason.NeedMasterPassword,
                    Group = info.Group,
                    Entry = info.Entry
                });
            }

            using (var secure = new Secure())
            {
                var userAccount = userAccountRepository.GetUserAccount(User);
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
                    group = new PasswordGroup { GroupName = info.Group };
                    passwordDb.PasswordGroups.Add(group);
                }
                var entry = group.PasswordEntries.FirstOrDefault(pe => pe.Name == info.Entry);
                if (entry == null)
                {
                    // no existing entry, adding a new one
                    entry = new PasswordEntry { Name = info.Entry };
                    group.PasswordEntries.Add(entry);
                }
                entry.IsValueEncrypted = info.ValueEncrypted;

                if (info.ValueEncrypted)
                {
                    var encryptedInfo = secure.Encrypt(masterPassword, passwordDb.IV, info.Value);
                    entry.Salt = encryptedInfo.Salt;
                    entry.Value = encryptedInfo.EncryptedValueBase64Encoded;
                    passwordDb.IV = passwordDb.IV ?? encryptedInfo.IV;
                }
                else
                {
                    entry.Salt = null;
                    entry.Value = info.Value;
                }

                userAccount.SetPasswordDb(passwordDb);
                userAccountRepository.SaveUserAccount(userAccount);

                return new ObjectResult(new {
                    Added = true,
                    Group = info.Group,
                    Entry = info.Entry
                });
            }
        }
    }
}