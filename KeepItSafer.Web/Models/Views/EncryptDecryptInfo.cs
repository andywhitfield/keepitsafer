namespace KeepItSafer.Web.Models.Views
{
    public class EncryptDecryptInfo
    {
        public string Group { get; set; }
        public string Entry { get; set; }
        public string MasterPassword { get; set; }
        public bool RememberMasterPassword { get; set; }
    }
}