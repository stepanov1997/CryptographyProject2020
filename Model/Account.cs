using System;
using System.Security.Cryptography;
using System.Text;

namespace CryptographyProject2019.Model
{
    [Serializable]
    public class Account
    { 
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PathToCertificate { get; set; }

        public Account(string username, string password, string pathToCertificate)
        {
            Username = username;
            HashAlgorithm hashAlg = new MD5CryptoServiceProvider();
            var data = Encoding.Unicode.GetBytes(password);
            PasswordHash = Encoding.Unicode.GetString(hashAlg.ComputeHash(data));
            PathToCertificate = pathToCertificate;
        }

    }
}