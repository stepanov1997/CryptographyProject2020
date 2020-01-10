using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptographyProject2019.Model;

namespace CryptographyProject2019.Controller
{
    static class SessionController
    {
        private static Bitmap bitmap = Steganography.CreateNonIndexedImage(Image.FromFile(Directory.GetCurrentDirectory() + $@"\..\..\Resources\MonaLisa.jpg"));
        public static bool CreateSessionRequestResponse(Account sender, Account receiver, bool isResponse, object locker)
        {
            //========================================================================
            // Validate sender and receiver certificate.
            var senderAccount = AccountsController.GetInstance().CurrentAccount;
            var senderCertificate = new X509Certificate2();
            senderCertificate.Import(senderAccount.PathToCertificate);

            var receiverCertificate = new X509Certificate2();
            receiverCertificate.Import(receiver.PathToCertificate);

            if (!ValidateController.ValidateCertificates(receiverCertificate)) return false;
            if (!ValidateController.ValidateCertificates(senderCertificate)) return false;

            //========================================================================
            // Encrypt sender username with receiver public key and save it into textfile.
            // Receiver can only read it, only he has his private key.
            var rsaprovider = (RSACryptoServiceProvider)receiverCertificate.PublicKey.Key;
            var encryptedSenderName =
                Convert.ToBase64String(rsaprovider.Encrypt(Encoding.Unicode.GetBytes(senderAccount.Username), false));
            string path = Directory.GetCurrentDirectory() +
                          $@"\..\..\ChatRequests\{receiver.Username}.{(isResponse ? "sesres" : "sesreq")}";
            lock (locker)
            {
                while (true)
                {
                    try
                    {
                        var stegBitmap = Steganography.MergeText(encryptedSenderName, bitmap);
                        stegBitmap.Save(path);
                        break;
                    }
                    catch (Exception)
                    {
                        Task.Delay(1000);
                    }
                }
            }
            return true;
        }

        public static Account ReadSessionRequest(Account receiver, bool isResponse, object locker)
        {
            var file = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\..\..\ChatRequests", isResponse ? "*.sesres" : "*.sesreq")
                .FirstOrDefault(e => Path.GetFileNameWithoutExtension(e) == receiver.Username);
            if (file == null) return null;
            string content;
            lock (locker)
            {
                while (true)
                {
                    try
                    {
                        content = Steganography.ExtractText(
                            new Bitmap(Image.FromFile(file))
                        );
                        break;
                    }
                    catch (Exception)
                    {
                        Task.Delay(1000);
                    }
                }
            }
            //========================================================================
            // Decrypt encrypted sender username with private key.
            var rsa = EncryptController.ImportPrivateKey(Directory.GetCurrentDirectory() +
                                                         $@"\..\..\CurrentUsers\{AccountsController.GetInstance().CurrentAccount.Username}.key");
            var senderUsername =
                Encoding.Unicode.GetString(rsa.Decrypt(Convert.FromBase64String(content), false));
            //========================================================================
            // Validate sender and receiver certificate.
            var senderAccount = AccountsController.GetInstance().CurrentAccount;
            var senderCertificate = new X509Certificate2();
            senderCertificate.Import(senderAccount.PathToCertificate);

            var receiverCertificate = new X509Certificate2();
            receiverCertificate.Import(receiver.PathToCertificate);

            if (!ValidateController.ValidateCertificates(receiverCertificate)) return null;
            if (!ValidateController.ValidateCertificates(senderCertificate)) return null;

            return AccountsController.GetInstance().Accounts[senderUsername];
        }
    }
}
