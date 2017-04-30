using System.Text;
using KeepItSafer.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeepItSafer.Web.Controllers
{
    public class DownloadController : Controller
    {
        private readonly IUserAccountRepository userAccountRepository;

        public DownloadController(IUserAccountRepository userAccountRepository)
        {
            this.userAccountRepository = userAccountRepository;
        }
        
        [Authorize]
        public IActionResult Index()
        {
            var passwordDb = userAccountRepository.GetUserAccount(User).PasswordDatabase;
            return File(Encoding.ASCII.GetBytes(passwordDb), "text/plain", "passworddb.txt");
        }
    }
}