using System.Security.Claims;
using KeepItSafer.Web.Models;

namespace KeepItSafer.Web.Data
{
    public interface IUserAccountRepository
    {
        bool HasMasterPassword(ClaimsPrincipal user);
        void CreateNewUser(ClaimsPrincipal user, string masterPassword);
        UserAccount GetUserAccount(ClaimsPrincipal user);
        void SaveUserAccount(UserAccount userAccount);
    }
}