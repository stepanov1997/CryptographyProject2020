using System;
using System.Security.Cryptography;
using System.Text;

namespace CryptographyProject2019.Model
{
    [Serializable]
    public class Account
    {
        public Account(string username, string password, string pathToCertificate)
        {
            Username = username;
            HashAlgorithm hashAlg = new MD5CryptoServiceProvider();
            var data = Encoding.Unicode.GetBytes(password);
            PasswordHash = Encoding.Unicode.GetString(hashAlg.ComputeHash(data));
            PathToCertificate = pathToCertificate;
        }

        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string PathToCertificate { get; set; }

        public void CreateDigitalSignature()
        {
            byte[] hashValue =
                {59, 4, 248, 102, 77, 97, 142, 201, 210, 12, 224, 93, 25, 41, 100, 197, 213, 134, 130, 135};

            //The value to hold the signed value.
            byte[] signedHashValue;

            //Generate a public/private key pair.
            var rsa = new RSACryptoServiceProvider();

            //Create an RSAPKCS1SignatureFormatter object and pass it the
            //RSACryptoServiceProvider to transfer the private key.
            var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);

            //Set the hash algorithm to SHA1.
            rsaFormatter.SetHashAlgorithm("SHA1");

            //Create a signature for hashValue and assign it to
            //signedHashValue.
            signedHashValue = rsaFormatter.CreateSignature(hashValue);
        }
    }
}