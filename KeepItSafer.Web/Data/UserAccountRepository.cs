using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using KeepItSafer.Crypto;
using KeepItSafer.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KeepItSafer.Web.Data
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly SqliteDataContext context;
        private readonly ILogger<UserAccountRepository> logger;

        public UserAccountRepository(SqliteDataContext context, ILogger<UserAccountRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<bool> HasMasterPasswordAsync(ClaimsPrincipal user)
        {
            var userAccount = await GetUserAccountOrNullAsync(user);
            return userAccount != null && !string.IsNullOrWhiteSpace(userAccount.PasswordDatabase);
        }

        public Task CreateNewUserAsync(ClaimsPrincipal user, string masterPassword)
        {
            var authenticationUri = GetIdentifierFromPrincipal(user);
            var newAccount = new UserAccount { AuthenticationUri = authenticationUri };

            using (var secure = new Secure())
            {
                newAccount.SetPasswordDb(new PasswordDb {
                    MasterPassword = secure.HashValue(masterPassword)
                });
            }

            context.UserAccounts.Add(newAccount);
            return context.SaveChangesAsync();
        }

        private string GetIdentifierFromPrincipal(ClaimsPrincipal user)
        {
            return user?.FindFirstValue("sub");
        }

        public Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user)
        {
            return GetUserAccountOrNullAsync(user) ?? throw new ArgumentException($"No UserAccount for the user: {user}");
        }

        private Task<UserAccount> GetUserAccountOrNullAsync(ClaimsPrincipal user)
        {
            var authenticationUri = GetIdentifierFromPrincipal(user);
            if (string.IsNullOrWhiteSpace(authenticationUri))
                return null;

            return context.UserAccounts.FirstOrDefaultAsync(ua => ua.AuthenticationUri == authenticationUri);
        }

        public async Task SaveUserAccountAsync(UserAccount userAccount)
        {
            context.UserAccounts.Update(userAccount);
            await context.SaveChangesAsync();

            if (string.IsNullOrEmpty(userAccount.DropboxToken))
                return;

            using var contentStream = new MemoryStream(Encoding.ASCII.GetBytes(userAccount.PasswordDatabase));
            using var dropboxClient = new DropboxClient(userAccount.DropboxToken);
            var file = await dropboxClient.Files.UploadAsync(
                "/keepitsafer.db.json",
                WriteMode.Overwrite.Instance,
                body: contentStream);
            logger.LogTrace($"Saved {file.PathDisplay}/{file.Name} rev {file.Rev}");
        }
    }
}