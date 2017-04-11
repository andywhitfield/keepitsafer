using KeepItSafer.Web.Models.Views;
using Microsoft.AspNetCore.Mvc;

namespace KeepItSafer.Web.Controllers.Api
{
    [Route("api/[controller]")]
    public class DecryptController : Controller
    {
        [HttpPost]
        public IActionResult Decrypt([FromForm] DecryptItem item)
        {
            return new ObjectResult(new {
                Decrypted = true,
                DecryptedValue = "hello world"
            });
        }
    }
}