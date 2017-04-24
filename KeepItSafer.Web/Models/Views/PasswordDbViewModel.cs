using System.Collections.Generic;
using System.Linq;
using KeepItSafer.Crypto;

namespace KeepItSafer.Web.Models.Views
{
    public class PasswordDbViewModel
    {
        public PasswordDbViewModel(PasswordDb db)
        {
            Groups = db.PasswordGroups.Select(pg => new GroupViewModel(pg)).OrderBy(pg => pg.Name);
        }

        public IEnumerable<GroupViewModel> Groups { get; }
    }
}