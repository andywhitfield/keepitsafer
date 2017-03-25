using KeepItSafer.Web.Crypto;
using Xunit;

namespace KeepItSafer.Web.Tests.Crypto
{
    public class SecureTest
    {
        [Fact]
        public void TestEncryptThenDecrypt()
        {
            using (var secure = new Secure())
            {
                var encrypted = secure.Encrypt("master password", null, "value to encrypt");
                Assert.NotNull(encrypted);
                Assert.NotEqual(encrypted.EncryptedValueBase64Encoded, "value to encrypt");

                var decrypted = secure.Decrypt("master password", encrypted.IVBase64Encoded, encrypted.SaltBase64Encoded, encrypted.EncryptedValueBase64Encoded);
                Assert.Equal("value to encrypt", decrypted);
            }
        }
    }
}
