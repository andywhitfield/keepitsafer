using Xunit;

namespace KeepItSafer.Crypto.Tests
{
    public class SecureTest
    {
        [Fact]
        public void TestEncryptThenDecrypt()
        {
            (string EncryptedValueBase64Encoded, byte[] IV, byte[] Salt) encrypted;
            using (var secure = new Secure())
            {
                encrypted = secure.Encrypt("master password", null, "value to encrypt");
                Assert.NotEqual("value to encrypt", encrypted.EncryptedValueBase64Encoded);

                var decrypted = secure.Decrypt("master password", encrypted.IV, encrypted.Salt, encrypted.EncryptedValueBase64Encoded);
                Assert.Equal("value to encrypt", decrypted);
            }

            using (var secure = new Secure())
            {
                var decrypted = secure.Decrypt("master password", encrypted.IV, encrypted.Salt, encrypted.EncryptedValueBase64Encoded);
                Assert.Equal("value to encrypt", decrypted);
            }
        }

        [Fact]
        public void TestPasswordHashing()
        {
            string valueToHash = "Some nice long password!";
            string hash;
            using (var secure = new Secure())
            {
                hash = secure.HashValue(valueToHash);
                Assert.NotEqual(valueToHash, hash);

                Assert.True(secure.ValidateHash(valueToHash, hash));
            }

            using (var secure = new Secure())
            {
                Assert.True(secure.ValidateHash(valueToHash, hash));
            }

            using (var secure = new Secure())
            {
                hash = secure.HashValue(valueToHash);
            }
            using (var secure = new Secure())
            {
                Assert.True(secure.ValidateHash(valueToHash, hash));
            }
        }
    }
}
