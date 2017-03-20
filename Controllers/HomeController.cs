using System.Linq;
using KeepItSafer.Data;
using Microsoft.AspNetCore.Mvc;

namespace KeepItSafer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            using (var db = new SqliteDataContext())
            {
                ViewData["Users"] = string.Join(", ", db.UserAccounts.Select(ua => $"{ua.UserAccountId}:{ua.AuthenticationUri}"));
            }

            return View();
        }

        public IActionResult Error() => View();
    }
}
