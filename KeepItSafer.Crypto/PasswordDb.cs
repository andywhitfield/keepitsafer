using System.Collections.Generic;

namespace KeepItSafer.Crypto
{
    public class PasswordDb
    {
        public string MasterPassword { get; set; }
        public byte[] IV { get; set; }
        public List<PasswordGroup> PasswordGroups { get; set; } = new List<PasswordGroup>();
    }
}