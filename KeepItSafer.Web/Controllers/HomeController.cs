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
        public IActionResult Index()
        {
            if (!userAccountRepository.HasMasterPassword(User))
            {
                return Redirect("~/newuser");
            }

            return View(new PasswordDbViewModel(userAccountRepository.GetUserAccount(User).GetPasswordDb()));
        }

        public IActionResult Error() => View();
    }
}
