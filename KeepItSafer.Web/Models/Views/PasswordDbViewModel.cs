using System.Collections.Generic;
using System.Linq;
using KeepItSafer.Crypto;

namespace KeepItSafer.Web.Models.Views
{
    public class PasswordDbViewModel
    {
        public PasswordDbViewModel(PasswordDb db, string dropboxToken)
        {
            Groups = db.PasswordGroups.Select(pg => new GroupViewModel(pg)).OrderBy(pg => pg.Name);
            HasDropboxToken = !string.IsNullOrEmpty(dropboxToken);
        }

        public IEnumerable<GroupViewModel> Groups { get; }
        public bool HasDropboxToken { get; }
    }
}