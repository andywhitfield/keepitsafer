using System.Linq;
using KeepItSafer.Web.Data;
using KeepItSafer.Web.Models;
using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace KeepItSafer.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFileProvider fileProvider;
        private readonly IUserAccountRepository userAccountRepository;

        public HomeController(IFileProvider fileProvider, IUserAccountRepository userAccountRepository)
        {
            this.fileProvider = fileProvider;
            this.userAccountRepository = userAccountRepository;
        }
        
        [Authorize]
        public IActionResult Index()
        {
            if (!userAccountRepository.HasMasterPassword(User))
            {
                return Redirect("~/newuser");
            }

            ViewData["dicts"] = string.Join(", ", fileProvider.GetDirectoryContents("Dictionary").Select(dc => dc.Name));

            return View(new PasswordDbViewModel(userAccountRepository.GetUserAccount(User).GetPasswordDb()));
        }

        public IActionResult Error() => View();
    }
}
