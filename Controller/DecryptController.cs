using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace CryptographyProject2019.Controller
{
    internal class DecryptController
    {
        public static string DecryptEncryptedFile(EncryptedFileParameters @params)
        {
            //========================================================================
            // Decrypt encrypted symmetric key with private key.
            var rsa = EncryptController.ImportPrivateKey(Directory.GetCurrentDirectory() +
                                                         $"/../../CurrentUsers/{AccountsController.GetInstance().CurrentAccount.Username}.key");
            var symmetricKey =
                Encoding.Unicode.GetString(rsa.Decrypt(Convert.FromBase64String(@params.EncryptedSymmetricKey), false));

            //========================================================================
            // Decrypt encrypted sendername.
            var decryptedName = Cipher.Decrypt(@params.EncryptedSenderName, symmetricKey, @params.SymmetricAlgorithm);

            //========================================================================
            // Validate sender and receiver certificate.
            var senderAccount = AccountsController.GetInstance().Accounts[decryptedName];
            var senderCertificate = new X509Certificate2();
            senderCertificate.Import(senderAccount.PathToCertificate);

            var receiverAccount = AccountsController.GetInstance().CurrentAccount;
            var receiverCertificate = new X509Certificate2();
            receiverCertificate.Import(receiverAccount.PathToCertificate);

            if (!ValidateController.ValidateCertificates(receiverCertificate))
                return "";
            if (!ValidateController.ValidateCertificates(senderCertificate))
                return "";

            //========================================================================
            // Decrypt digital signature with symmetric key.
            var digitalSignature = Convert.FromBase64String(
                Cipher.Decrypt(@params.EncryptedSignature, symmetricKey, @params.SymmetricAlgorithm));

            //========================================================================
            // Decrypt data with symmetric key.
            var decryptedData = Cipher.Decrypt(@params.EncryptedData, symmetricKey, @params.SymmetricAlgorithm);

            //========================================================================
            // Verify messagehash with signature.
            var decryptedHashAlg = Cipher.Decrypt(@params.EncryptedHashAlg, symmetricKey, @params.SymmetricAlgorithm);
            var rsaCrypto = (RSACryptoServiceProvider)senderCertificate.PublicKey.Key;
            if (!VerifyData(Encoding.Unicode.GetBytes(decryptedData), digitalSignature, rsaCrypto.ExportParameters(false), decryptedHashAlg))
            {
                return null;
            }

            //========================================================================
            // Return a message.

            return decryptedData;
        }

        public static EncryptedFileParameters EncryptedFileParametersParser(string path, object locker)
        {
            var ext = Path.GetExtension(path);
            ext = ext?.Substring(1);
            SymmetricAlgorithm symmetricAlgorithm = EncryptController.CheckSymmetricAlgorithm(ext);

            string content;
            lock (locker)
            {
                while (true)
                {
                    try
                    {
                        content = File.ReadAllText(path);
                        break;
                    }
                    catch (IOException)
                    {
                        Task.Delay(1000);
                    }
                }
            }

            var match = Regex.Match(content,
                "ENCRYPTED SYMMETRIC KEY:\n(.*?)\n\n" +
                "ENCRYPTED HASH ALGORITHM:\n(.*?)\n\n" +
                "ENCRYPTED DATA SENDER:\n(.*?)\n\n" +
                "ENCRYPTED DIGITAL SIGNATURE:\n(.*?)\n\n" +
                "ENCRYPTED DATA:\n(.*?)\n\n");

            var @params = new EncryptedFileParameters
            {
                EncryptedSymmetricKey = match.Groups[1].Value,
                EncryptedHashAlg = match.Groups[2].Value,
                EncryptedSenderName = match.Groups[3].Value,
                EncryptedSignature = match.Groups[4].Value,
                EncryptedData = match.Groups[5].Value,
                SymmetricAlgorithm = symmetricAlgorithm
            };

            return @params;
        }

        private static bool VerifyData(byte[] originalMessage, byte[] signedMessage, RSAParameters publicKey,
            string hashAlgorithm)
        {
            var success = false;
            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.ImportParameters(publicKey);
                    success = rsa.VerifyData(originalMessage, CryptoConfig.MapNameToOID(hashAlgorithm), signedMessage);
                }
                catch (CryptographicException e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }

            return success;
        }

        private static string FromBase64(string encoded)
        {
            return Encoding.Unicode.GetString(Convert.FromBase64String(encoded));
        }

        public struct EncryptedFileParameters
        {
            public string EncryptedSymmetricKey;
            public string EncryptedHashAlg;
            public string EncryptedSenderName;
            public string EncryptedSignature;
            public string EncryptedData;
            public SymmetricAlgorithm SymmetricAlgorithm;
        }
    }
}