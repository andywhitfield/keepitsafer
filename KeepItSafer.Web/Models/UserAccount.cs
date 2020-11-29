namespace KeepItSafer.Web.Models
{
    public class UserAccount
    {
        public int UserAccountId { get; set; }
        public string AuthenticationUri { get; set; }
        public string PasswordDatabase { get; set; }
        public string DropboxAccessToken { get; set; }
        public string DropboxRefreshToken { get; set; }
    }
}
