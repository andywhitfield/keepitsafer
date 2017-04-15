using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace KeepItSafer.Web.Controllers
{
    public static class ControllerExtensions
    {
        private const string RememberMasterPasswordSessionKey = "RememberMasterPasswordSessionKey";

        public static string RememberedMasterPassword(this Controller controller)
        {
            var session = controller.HttpContext.Session;
            if (!session.IsAvailable)
            {
                return null;
            }
            if (!session.TryGetValue(RememberMasterPasswordSessionKey, out var encodedPassword))
            {
                return null;
            }
            return Encoding.Unicode.GetString(encodedPassword);
        }

        public static void RememberMasterPassword(this Controller controller, string masterPasswordToRemember)
        {
            var session = controller.HttpContext.Session;
            if (!session.IsAvailable)
            {
                return;
            }
            session.Set(RememberMasterPasswordSessionKey, Encoding.Unicode.GetBytes(masterPasswordToRemember));
        }
    }
}