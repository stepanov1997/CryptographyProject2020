using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using Org.BouncyCastle.X509;

namespace CryptographyProject2019.Controller
{
    internal class ValidateController
    {
        public static bool ValidateCertificates(X509Certificate2 cert)
        {
            var pathCA = Directory.GetCurrentDirectory() + "/../../CryptoFiles/rootca.pem";
            var caCertificate = new X509CertificateParser().ReadCertificate(File.ReadAllBytes(pathCA));
            var pathCRL = Directory.GetCurrentDirectory() + "/../../CryptoFiles/crl/list.pem";
            var crl = new X509CrlParser().ReadCrl(File.ReadAllBytes(pathCRL));
            var receiverCert = new X509CertificateParser().ReadCertificate(cert.GetRawCertData());

            try
            {
                receiverCert.Verify(caCertificate.GetPublicKey());
            }
            catch
            {
                MessageBox.Show("Receiver's certificate is not signed by CA!");
                return false;
            }

            if (crl.IsRevoked(receiverCert))
            {
                MessageBox.Show("Receiver's certificate is revoked!");
                return false;
            }

            return true;
        }
    }
}