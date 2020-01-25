using System.Security.Claims;
using System.Threading.Tasks;
using KeepItSafer.Web.Models;

namespace KeepItSafer.Web.Data
{
    public interface IUserAccountRepository
    {
        Task<bool> HasMasterPasswordAsync(ClaimsPrincipal user);
        Task CreateNewUserAsync(ClaimsPrincipal user, string masterPassword);
        Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user);
        Task SaveUserAccountAsync(UserAccount userAccount);
    }
}