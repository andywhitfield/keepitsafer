using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KeepItSafer.Crypto
{
    public class Secure : IDisposable
    {
        private readonly SymmetricAlgorithm algorithm;
        private readonly RandomNumberGenerator random = RandomNumberGenerator.Create();

        public Secure()
        {
            algorithm = Aes.Create();
        }

        public (string EncryptedValueBase64Encoded, byte[] IV, byte[] Salt) Encrypt(
            string password, byte[] iv, string unencryptedValue)
        {
            if (iv == null || iv.Length == 0)
            {
                algorithm.GenerateIV();
                iv = algorithm.IV;
            }
            else
            {
                algorithm.IV = iv;
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
                    return (Convert.ToBase64String(encryptionStreamBacking.ToArray()), iv, salt);
                }
            }
        }

        public string Decrypt(string password, byte[] iv, byte[] salt, string encryptedValueBase64Encoded)
        {
            algorithm.IV = iv;
            using (var passwordToBytes = new Rfc2898DeriveBytes(password, algorithm.KeySize))
            {
                passwordToBytes.Salt = salt;
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

        public string HashValue(string value)
        {
            return BCrypt.Net.BCrypt.HashPassword(value);
        }

        public bool ValidateHash(string value, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(value, hash);
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