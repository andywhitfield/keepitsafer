using System.Collections.Generic;

namespace KeepItSafer.Crypto
{
    public class PasswordDb
    {
        public PasswordDb()
        {
            PasswordGroups = new List<PasswordGroup>();
        }

        public string MasterPassword { get; set; }
        public byte[] IV { get; set; }
        public ICollection<PasswordGroup> PasswordGroups { get; private set; }
    }
}