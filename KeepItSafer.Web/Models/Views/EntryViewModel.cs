using KeepItSafer.Crypto;

namespace KeepItSafer.Web.Models.Views
{
    public class EntryViewModel
    {
        public EntryViewModel(PasswordEntry entry)
        {
            Name = entry.Name;
            Value = entry.IsValueEncrypted ? string.Empty : entry.Value;
            IsValueEncrypted = entry.IsValueEncrypted;
        }

        public string Name { get; }
        public string Value { get; }
        public bool IsValueEncrypted { get; }
    }
}