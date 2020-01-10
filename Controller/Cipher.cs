using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptographyProject2019.Controller
{
    public static class Cipher
    {
        /// <summary>
        ///     Encrypt a string.
        /// </summary>
        /// <param name="plainText">String to be encrypted</param>
        /// <param name="password">Password</param>
        /// <param name="symmetricAlgorithm"></param>
        public static string Encrypt(string plainText, string password, SymmetricAlgorithm symmetricAlgorithm)
        {
            if (plainText == null) return null;

            if (password == null) password = string.Empty;

            // Get the bytes of the string
            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes, symmetricAlgorithm);

            return Convert.ToBase64String(bytesEncrypted);
        }

        /// <summary>
        ///     Decrypt a string.
        /// </summary>
        /// <param name="encryptedText">String to be decrypted</param>
        /// <param name="password">Password used during encryption</param>
        /// <param name="symmetricAlgorithm"></param>
        /// <exception cref="FormatException"></exception>
        public static string Decrypt(string encryptedText, string password, SymmetricAlgorithm symmetricAlgorithm)
        {
            if (encryptedText == null) return null;

            if (password == null) password = string.Empty;

            // Get the bytes of the string
            var bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            var bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes, symmetricAlgorithm);

            return Encoding.UTF8.GetString(bytesDecrypted);
        }

        private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes,
            SymmetricAlgorithm symmetricAlgorithm)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] {1, 2, 3, 4, 5, 6, 7, 8};

            using (var ms = new MemoryStream())
            {
                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                SetSizes(symmetricAlgorithm);

                symmetricAlgorithm.Key = key.GetBytes(symmetricAlgorithm.KeySize / 8);
                symmetricAlgorithm.IV = key.GetBytes(symmetricAlgorithm.BlockSize / 8);

                symmetricAlgorithm.Mode = CipherMode.CBC;

                using (var cs = new CryptoStream(ms, symmetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                    cs.Close();
                }

                encryptedBytes = ms.ToArray();
            }

            return encryptedBytes;
        }

        private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes,
            SymmetricAlgorithm symmetricAlgorithm)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] {1, 2, 3, 4, 5, 6, 7, 8};

            using (var ms = new MemoryStream())
            {
                var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                SetSizes(symmetricAlgorithm);

                symmetricAlgorithm.Key = key.GetBytes(symmetricAlgorithm.KeySize / 8);
                symmetricAlgorithm.IV = key.GetBytes(symmetricAlgorithm.BlockSize / 8);
                symmetricAlgorithm.Mode = CipherMode.CBC;

                using (var cs = new CryptoStream(ms, symmetricAlgorithm.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                    cs.Close();
                }

                decryptedBytes = ms.ToArray();
            }

            return decryptedBytes;
        }

        public static void SetSizes(SymmetricAlgorithm symmetricAlgorithm)
        {
            if (symmetricAlgorithm is Aes)
            {
                symmetricAlgorithm.KeySize = 256;
                symmetricAlgorithm.BlockSize = 128;
            }
            else if (symmetricAlgorithm is RC2)
            {
                symmetricAlgorithm.KeySize = 128;
                symmetricAlgorithm.BlockSize = 64;
            }
            else if (symmetricAlgorithm is TripleDES)
            {
                symmetricAlgorithm.KeySize = 192;
                symmetricAlgorithm.BlockSize = 64;
            }
        }
    }
}