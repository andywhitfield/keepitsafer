using System.Linq;
using KeepItSafer.Web.Models;
using Xunit;

namespace KeepItSafer.Web.Tests.Models
{
    public class PasswordDbTest
    {
        [Fact]
        public void TestSetAndGet()
        {
            var group1 = new PasswordGroup { GroupName = "group 1", PasswordEntries = {
                new PasswordEntry { Name = "entry 1.1", IsValueEncrypted = false, Value = "plain text 1.1" },
                new PasswordEntry { Name = "entry 1.2", IsValueEncrypted = true, Value = "encrypted value 1.2", Salt = new byte[] { 1, 2 } }
            }};

            var group2 = new PasswordGroup { GroupName = "group 2", PasswordEntries = {
                new PasswordEntry { Name = "entry 2.1", IsValueEncrypted = true, Value = "encrypted value 2.1", Salt = new byte[] { 2, 1 } }
            }};

            var db = new PasswordDb {
                MasterPassword = "hashed password",
                IV = new byte[] { 1, 2, 3, 4},
                PasswordGroups = { group1, group2 }
            };

            var account = new UserAccount();
            Assert.Null(account.PasswordDatabase);

            var dbService = new PasswordDbService();
            dbService.SetPasswordDb(db, account);

            Assert.NotNull(account.PasswordDatabase);
            Assert.NotEqual("", account.PasswordDatabase);

            var deserialized = dbService.GetPasswordDb(account);
            Assert.NotNull(deserialized);

            Assert.Equal("hashed password", deserialized.MasterPassword);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, deserialized.IV);
            Assert.Equal(2, deserialized.PasswordGroups.Count);

            Assert.Equal("group 1", deserialized.PasswordGroups.ElementAt(0).GroupName);
            Assert.Equal(2, deserialized.PasswordGroups.ElementAt(0).PasswordEntries.Count);

            Assert.Equal("entry 1.1", deserialized.PasswordGroups.ElementAt(0).PasswordEntries.ElementAt(0).Name);
            Assert.Equal(false, deserialized.PasswordGroups.ElementAt(0).PasswordEntries.ElementAt(0).IsValueEncrypted);
            Assert.Equal("plain text 1.1", deserialized.PasswordGroups.ElementAt(0).PasswordEntries.ElementAt(0).Value);
            Assert.Equal(null, deserialized.PasswordGroups.ElementAt(0).PasswordEntries.ElementAt(0).Salt);

            Assert.Equal("entry 1.2", deserialized.PasswordGroups.ElementAt(0).PasswordEntries.ElementAt(1).Name);
            Assert.Equal(true, deserialized.PasswordGroups.ElementAt(0).PasswordEntries.ElementAt(1).IsValueEncrypted);
            Assert.Equal("encrypted value 1.2", deserialized.PasswordGroups.ElementAt(0).PasswordEntries.ElementAt(1).Value);
            Assert.Equal(new byte[] { 1, 2 }, deserialized.PasswordGroups.ElementAt(0).PasswordEntries.ElementAt(1).Salt);
            
            Assert.Equal("group 2", deserialized.PasswordGroups.ElementAt(1).GroupName);
            Assert.Equal(1, deserialized.PasswordGroups.ElementAt(1).PasswordEntries.Count);

            Assert.Equal("entry 2.1", deserialized.PasswordGroups.ElementAt(1).PasswordEntries.ElementAt(0).Name);
            Assert.Equal(true, deserialized.PasswordGroups.ElementAt(1).PasswordEntries.ElementAt(0).IsValueEncrypted);
            Assert.Equal("encrypted value 2.1", deserialized.PasswordGroups.ElementAt(1).PasswordEntries.ElementAt(0).Value);
            Assert.Equal(new byte[] { 2, 1 }, deserialized.PasswordGroups.ElementAt(1).PasswordEntries.ElementAt(0).Salt);
        }
    }
}