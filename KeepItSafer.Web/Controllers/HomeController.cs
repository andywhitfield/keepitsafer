using System.Threading.Tasks;
using KeepItSafer.Web.Data;
using KeepItSafer.Web.Models;
using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeepItSafer.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserAccountRepository userAccountRepository;

        public HomeController(IUserAccountRepository userAccountRepository)
        {
            this.userAccountRepository = userAccountRepository;
        }
        
        [Authorize]
        public async Task<ActionResult> Index()
        {
            if (!await userAccountRepository.HasMasterPasswordAsync(User))
            {
                return Redirect("~/newuser");
            }

            var userAccount = await userAccountRepository.GetUserAccountAsync(User);
            return base.View(new PasswordDbViewModel(userAccount.GetPasswordDb(), userAccount.DropboxToken));
        }

        public IActionResult Error() => View();
    }
}
