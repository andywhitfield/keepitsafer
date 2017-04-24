using System.Collections.Generic;
using System.Linq;
using KeepItSafer.Crypto;

namespace KeepItSafer.Web.Models.Views
{
    public class GroupViewModel
    {
        public GroupViewModel(PasswordGroup group)
        {
            Name = group.GroupName;
            Entries = group.PasswordEntries.Select(pe => new EntryViewModel(pe)).OrderBy(pe => pe.Name);
        }
        
        public string Name { get; }

        public IEnumerable<EntryViewModel> Entries { get; }
    }
}