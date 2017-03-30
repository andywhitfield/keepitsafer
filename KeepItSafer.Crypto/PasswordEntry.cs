namespace KeepItSafer.Crypto
{
    public class PasswordEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsValueEncrypted { get; set; }
        public byte[] Salt { get; set; }
    }
}