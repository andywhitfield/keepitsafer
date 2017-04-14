namespace KeepItSafer.Web.Models.Views
{
    public class EncryptDecryptInfo
    {
        public string Group { get; set; }
        public string Entry { get; set; }
        public string Value { get; set; }
        public bool ValueEncrypted { get; set; }
        public string MasterPassword { get; set; }
        public bool RememberMasterPassword { get; set; }

        public override string ToString()
        {
            return $"EncryptDecryptInfo[Group={Group};Entry={Entry};Value={Value};ValueEncrypted={ValueEncrypted};RememberMasterPassword={RememberMasterPassword}]";
        }
    }
}