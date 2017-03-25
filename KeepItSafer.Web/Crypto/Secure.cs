using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KeepItSafer.Web.Crypto
{
    public class Secure : IDisposable
    {
        private readonly SymmetricAlgorithm algorithm;
        private readonly RandomNumberGenerator random = RandomNumberGenerator.Create();

        public Secure()
        {
            algorithm = Aes.Create();
        }

        public (string EncryptedValueBase64Encoded, string IVBase64Encoded, string SaltBase64Encoded) Encrypt(
            string password, string ivBase64Encoded, string unencryptedValue)
        {
            if (string.IsNullOrEmpty(ivBase64Encoded))
            {
                algorithm.GenerateIV();
                ivBase64Encoded = Convert.ToBase64String(algorithm.IV);
            }
            else
            {
                algorithm.IV = Convert.FromBase64String(ivBase64Encoded);
            }

            using (var passwordToBytes = new Rfc2898DeriveBytes(password, algorithm.KeySize))
            {
                var salt = GenerateRandomSalt();
                passwordToBytes.Salt = salt;
                algorithm.Key = passwordToBytes.GetBytes(algorithm.KeySize / 8);
                using (var encryptionStreamBacking = new MemoryStream())
                {
                    using (var encrypt = new CryptoStream(encryptionStreamBacking, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        var unencryptedValueBytes = Encoding.Unicode.GetBytes(unencryptedValue);
                        encrypt.Write(unencryptedValueBytes, 0, unencryptedValueBytes.Length);
                        encrypt.FlushFinalBlock();
                    }
                    return (Convert.ToBase64String(encryptionStreamBacking.ToArray()),
                        ivBase64Encoded, Convert.ToBase64String(salt));
                }
            }
        }

        public string Decrypt(string password, string ivBase64Encoded, string saltBase64Encoded, string encryptedValueBase64Encoded)
        {
            algorithm.IV = Convert.FromBase64String(ivBase64Encoded);
            using (var passwordToBytes = new Rfc2898DeriveBytes(password, algorithm.KeySize))
            {
                passwordToBytes.Salt = Convert.FromBase64String(saltBase64Encoded);
                algorithm.Key = passwordToBytes.GetBytes(algorithm.KeySize / 8);
                using (var decryptionStreamBacking = new MemoryStream())
                {
                    using (var decrypt = new CryptoStream(decryptionStreamBacking, algorithm.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        var enryptedValueBytes = Convert.FromBase64String(encryptedValueBase64Encoded);
                        decrypt.Write(enryptedValueBytes, 0, enryptedValueBytes.Length);
                        decrypt.Flush();
                    }
                    return Encoding.Unicode.GetString(decryptionStreamBacking.ToArray());
                }
            }
        }

        private byte[] GenerateRandomSalt()
        {
            var salt = new byte[algorithm.KeySize];
            random.GetBytes(salt);
            return salt;
        }

        public void Dispose()
        {
            algorithm.Dispose();
            random.Dispose();
        }
    }
}