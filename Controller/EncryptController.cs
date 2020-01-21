using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptographyProject2019.Model;
using HttpKit;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace CryptographyProject2019.Controller
{
    public static class EncryptController
    {
        private static readonly Random RandomNumberGenerator = new Random();
        private static readonly string[] HashAlgorithms = { "SHA1", "SHA256" };
        private static readonly string[] SymmetricAlgorithms = { "RC2", "AES", "DES3" };
        public static bool EncryptFileAsync(string message, Account receiverAccount, object locker)
        {
            var senderUsername = AccountsController.GetInstance().CurrentAccount.Username;
            var receiverCertificatePath = receiverAccount.PathToCertificate;
            var receiverUsername = Path.GetFileNameWithoutExtension(receiverAccount.PathToCertificate);
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"/../../ReceivedMessages/{receiverUsername}/");

            //========================================================================
            // Validate sender and receiver certificate.
            var senderAccount = AccountsController.GetInstance().CurrentAccount;
            var senderCertificate = new X509Certificate2();
            senderCertificate.Import(senderAccount.PathToCertificate);

            var receiverCertificate = new X509Certificate2();
            receiverCertificate.Import(receiverCertificatePath);

            if (!ValidateController.ValidateCertificates(receiverCertificate)) return false;
            if (!ValidateController.ValidateCertificates(senderCertificate)) return false;

            //========================================================================
            // Generate symmetric key

            var randomSymmetricAlgorithm = RandomSymmetricAlgorithm();
            randomSymmetricAlgorithm.GenerateIV();
            randomSymmetricAlgorithm.GenerateKey();
            var symmetricKey = Encoding.Unicode.GetString(randomSymmetricAlgorithm.Key);

            //========================================================================
            // Encrypt symmetric key with receiver public key and save it into textfile.
            // Receiver can only read it, only he has his private key.
            var rsaprovider = (RSACryptoServiceProvider)receiverCertificate.PublicKey.Key;
            var encryptedSymmetricKey =
                Convert.ToBase64String(rsaprovider.Encrypt(Encoding.Unicode.GetBytes(symmetricKey), false));

            //========================================================================
            // Encrypt sender name and filename.

            var hashAlgorithm = RandomHashAlgorithm();
            var encryptedName = Cipher.Encrypt(senderUsername, symmetricKey, randomSymmetricAlgorithm);
            var encryptedHashAlg = Cipher.Encrypt(CheckHashAlgorithm(hashAlgorithm), symmetricKey, randomSymmetricAlgorithm);

            //========================================================================
            // Encrypt hash of file with user private key to make digital signature.
            var rsa = ImportPrivateKey(Directory.GetCurrentDirectory() + $"/../../CurrentUsers/{senderAccount.Username}.key");
            var signature = SignData(message, rsa.ExportParameters(true), hashAlgorithm);

            // Encrypt digital signature with symmetric key and save it as text file
            var encryptedSignature = Cipher.Encrypt(signature, symmetricKey, randomSymmetricAlgorithm);

            //========================================================================
            // Encrypt file data with symmetric key and save it as text file
            var encryptedData = Cipher.Encrypt(message, symmetricKey, randomSymmetricAlgorithm);

            //========================================================================
            var cryptedFile =
                $"ENCRYPTED SYMMETRIC KEY:\n{encryptedSymmetricKey}\n\n" +
                $"ENCRYPTED HASH ALGORITHM:\n{encryptedHashAlg}\n\n" +
                $"ENCRYPTED DATA SENDER:\n{encryptedName}\n\n" +
                $"ENCRYPTED DIGITAL SIGNATURE:\n{encryptedSignature}\n\n" +
                $"ENCRYPTED DATA:\n{encryptedData}\n\n";

            var path = MakePathOfCryptedFile(receiverUsername, CheckSymmetricAlgorithm(randomSymmetricAlgorithm));

            while (true)
            {
                try
                {
                    FileStream fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Write);
                    using (StreamWriter sr = new StreamWriter(new NonClosingStreamWrapper(fileStream)))
                    {
                        sr.Write(cryptedFile);
                    }
                    fileStream.Close();
                    break;
                }
                catch (Exception)
                {
                    Thread.Sleep(40);
                }
            }
            return true;
        }

        private static string MakePathOfCryptedFile(string toFolder, string algExt)
        {
            var i = 1;
            var numOfFile = "";
            while (File.Exists(
                $"{Directory.GetCurrentDirectory()}/../../ReceivedMessages/{toFolder}/message{numOfFile}.{algExt}"))
                numOfFile = (++i).ToString();
            return
                $"{Directory.GetCurrentDirectory()}/../../ReceivedMessages/{toFolder}/message{numOfFile}.{algExt}";
        }

        public static string CheckSymmetricAlgorithm(SymmetricAlgorithm symmetricAlgorithm)
        {
            switch (symmetricAlgorithm)
            {
                case Aes _:
                    return "AES";
                case RC2 _:
                    return "RC2";
                case TripleDES _:
                    return "DES3";
                default:
                    return "AES";
            }
        }

        private static HashAlgorithm RandomHashAlgorithm() =>
            CheckHashAlgorithm(HashAlgorithms[RandomNumberGenerator.Next(HashAlgorithms.Length)]);

        private static SymmetricAlgorithm RandomSymmetricAlgorithm() => CheckSymmetricAlgorithm(SymmetricAlgorithms[RandomNumberGenerator.Next(SymmetricAlgorithms.Length)]);

        public static string CheckHashAlgorithm(HashAlgorithm hashAlgorithm)
        {
            switch (hashAlgorithm)
            {
                case SHA1 _:
                    return "SHA1";
                case MD5 _:
                    return "MD5";
                case SHA256 _:
                    return "SHA256";
                case SHA384 _:
                    return "SHA384";
                case SHA512 _:
                    return "SHA512";
                default:
                    return "SHA384";
            }
        }

        public static HashAlgorithm CheckHashAlgorithm(string hashAlgorithm)
        {
            switch (hashAlgorithm)
            {
                case "SHA1":
                    return SHA1.Create();
                case "MD5":
                    return MD5.Create();
                case "SHA256":
                    return SHA256.Create();
                case "SHA384":
                    return SHA384.Create();
                case "SHA512":
                    return SHA512.Create();
                default:
                    return SHA384.Create();
            }
        }
        public static SymmetricAlgorithm CheckSymmetricAlgorithm(string symmetricAlgorithm)
        {
            switch (symmetricAlgorithm)
            {
                case "AES":
                    return Aes.Create();
                case "RC2":
                    return RC2.Create();
                case "DES3":
                    return TripleDES.Create();
                default:
                    return Aes.Create();
            }
        }

        public static RSACryptoServiceProvider ImportPrivateKey(string path)
        {
            var sr = new StreamReader(path);
            var pr = new PemReader(sr);
            var KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
            var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)KeyPair.Private);

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParams);
            return rsa;
        }

        private static string SignData(string message, RSAParameters privateKey, HashAlgorithm hashAlgorithm)
        {
            byte[] signedBytes;

            using (var rsa = new RSACryptoServiceProvider())
            {
                // Write the message to a byte array using ASCII as the encoding.
                var originalData = Encoding.Unicode.GetBytes(message);

                try
                {
                    // Import the private key used for signing the message
                    rsa.ImportParameters(privateKey);

                    string alg;
                    alg = hashAlgorithm is SHA256 ? "SHA256" : "SHA1";

                    signedBytes = rsa.SignData(originalData, CryptoConfig.MapNameToOID(alg));
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
                finally
                {
                    // Set the keycontainer to be cleared when rsa is garbage collected.
                    rsa.PersistKeyInCsp = false;
                }
            }

            // Convert the byte array back to a string message
            return Convert.ToBase64String(signedBytes);
        }
    }
}