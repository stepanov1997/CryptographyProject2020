using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace CryptographyProject2019
{
    public static class Program
    {
        public static void Run()
        {
            //    AccountsController.GetInstance().AddAccount(new Account("stepanov", "step3110", "Gothman/Batman/Beyond"));
        }


        public static (string, byte[]) MakeCertificate(string username, DateTimeOffset notBefore,
            DateTimeOffset notAfter)
        {
            var ecdsa = ECDsa.Create(); // generate asymmetric key pair
            var req = new CertificateRequest("cn=stepanov", ecdsa, HashAlgorithmName.SHA256);
            var cert = req.CreateSelfSigned(notBefore, notAfter);

            // Create PFX (PKCS #12) with private key
            var pfxBytes = cert.Export(X509ContentType.Pfx, "P@55w0rd");

            // Create Base 64 encoded CER (public key only)
            var cerString = Encoding.Unicode.GetString(cert.Export(X509ContentType.Cert));

            return (cerString, pfxBytes);
        }

        public static string ReadName(X509Certificate2 cert)
        {
            var username = cert.SubjectName.Name;
            var match = Regex.Match(username, "[cC][nN]=(.*?),");
            username = match.Groups[1].Value;
            username = Regex.Replace(username, @"\s+", ".");
            username = username.ToLower();
            return username;
        }
    }
}