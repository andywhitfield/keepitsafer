using System.Collections.Generic;

namespace KeepItSafer.Crypto
{
    public class PasswordGroup
    {
        public string GroupName { get; set; }
        public List<PasswordEntry> PasswordEntries { get; set; } = new List<PasswordEntry>();
    }
}