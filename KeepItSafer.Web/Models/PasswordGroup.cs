using System.Collections.Generic;

namespace KeepItSafer.Web.Models
{
    public class PasswordGroup
    {
        public PasswordGroup()
        {
            PasswordEntries = new List<PasswordEntry>();
        }

        public string GroupName { get; set; }
        public ICollection<PasswordEntry> PasswordEntries { get; private set; }
    }
}