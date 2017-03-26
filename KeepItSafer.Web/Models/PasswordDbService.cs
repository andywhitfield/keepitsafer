using Newtonsoft.Json;

namespace KeepItSafer.Web.Models
{
    public class PasswordDbService
    {
        public PasswordDb GetPasswordDb(UserAccount userAccount)
        {
            if (string.IsNullOrWhiteSpace(userAccount.PasswordDatabase))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<PasswordDb>(userAccount.PasswordDatabase);
        }

        public void SetPasswordDb(PasswordDb passwordDb, UserAccount userAccount)
        {
            userAccount.PasswordDatabase = JsonConvert.SerializeObject(passwordDb);
        }
    }
}