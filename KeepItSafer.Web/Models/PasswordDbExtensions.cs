using Newtonsoft.Json;
using KeepItSafer.Crypto;

namespace KeepItSafer.Web.Models
{
    public static class PasswordDbExtensions
    {
        public static PasswordDb GetPasswordDb(this UserAccount userAccount)
        {
            if (string.IsNullOrWhiteSpace(userAccount.PasswordDatabase))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<PasswordDb>(userAccount.PasswordDatabase);
        }

        public static void SetPasswordDb(this UserAccount userAccount, PasswordDb passwordDb)
        {
            userAccount.PasswordDatabase = JsonConvert.SerializeObject(passwordDb);
        }
    }
}