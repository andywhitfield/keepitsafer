using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using KeepItSafer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace KeepItSafer.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFileProvider fileProvider;

        public HomeController(IFileProvider fileProvider)
        {
            this.fileProvider = fileProvider;
        }
        
        public IActionResult Index()
        {
            using (var db = new SqliteDataContext())
            {
                ViewData["Users"] = string.Join(", ", db.UserAccounts.Select(ua => $"{ua.UserAccountId}:{ua.AuthenticationUri}"));
            }
            ViewData["dicts"] = string.Join(", ", fileProvider.GetDirectoryContents("Dictionary").Select(dc => dc.Name));

            try
            {
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword("*** MY MASTER PASSWORD ***");
                ViewData["hashed"] = $"hash: {hashedPassword}";
            }
            catch (Exception ex)
            {
                ViewData["hashed"] = $"ex: {ex}";
            }

            try
            {
                var algorithm = Aes.Create();
                algorithm.IV = Convert.FromBase64String("brsg786LoK5r4CJVNRW9vA==");
                string decrypted;

                using (var passwordToBytes = new Rfc2898DeriveBytes("*** MY MASTER PASSWORD ***", algorithm.KeySize))
                {
                    passwordToBytes.Salt = Convert.FromBase64String("t/1r6xvs8iWxMT0yUJjkLLfwk2sGgR701qMwqnC2pFgFs3PA+nhLhVZ29jpuTVW6+PT+u6iWXgA2rddC79En99KOD5+nY9SxgNaaDNaf3nnuKjkHhO0uSKGfZrR7rI+OkcRX6MTsRHC9b/W0MVmzRP77gwh3/ybOx1aW+1jVbZy2j/xicKhgu4FHG7XDPjMQfifTkUdoEvoDhf7IuuaokhSqKvcCW9uvnOyMMJcvzvjaHasfC/j02QMP+rTFKhM0dunoPU2kQ2zOVAXzLLyO4q/3ZGBaSyPOCu67kglY9iguDAE5R9kthIxfE5Zal3AqKK6KlUR/azLeNRDf2hIFuA==");
                    algorithm.Key = passwordToBytes.GetBytes(algorithm.KeySize / 8);
                    using (var decryptionStreamBacking = new MemoryStream())
                    {
                        using (var decrypt = new CryptoStream(decryptionStreamBacking, algorithm.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            var enryptedValueBytes = Convert.FromBase64String("LxPqS/ZGuoK3ULlS1ArTdZPo7rGVSDOKPIKuPDQ/G7Y=");
                            decrypt.Write(enryptedValueBytes, 0, enryptedValueBytes.Length);
                            decrypt.Flush();
                        }
                        decrypted = Encoding.Unicode.GetString(decryptionStreamBacking.ToArray());
                    }
                }

                ViewData["decrypt"] = decrypted;
            }
            catch (Exception ex)
            {
                ViewData["decrypt"] = $"ex: {ex}";
            }

            return View();
        }

        public IActionResult Error() => View();
    }
}
