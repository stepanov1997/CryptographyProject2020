using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace CryptographyProject2019.View
{
    /// <summary>
    ///     Interaction logic for MakeCertificateWindow.xaml
    /// </summary>
    public partial class MakeCertificateWindow : Window
    {
        private readonly Window _previousWindow;

        public MakeCertificateWindow(Window previousWindow)
        {
            _previousWindow = previousWindow;
            InitializeComponent();
        }

        private void CreateCertificate(object sender, RoutedEventArgs e)
        {
            var userName = UsernameTextBox3.Text;
            if (userName == string.Empty)
            {
                MessageBox.Show("Please, type username!", "Username is missing", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var oldDirectory = Directory.GetCurrentDirectory();
            const string newDirectory = "../../CryptoFiles";
            Directory.SetCurrentDirectory(newDirectory);
            var makeKey = $"openssl genrsa -out private/{userName}.key";
            var makeReq =
                $"openssl req -new -key private/{userName}.key -config openssl.cnf -days 365 -out requests/{userName}_req.csr";
            var signCert =
                $"openssl ca -in requests/{userName}_req.csr -config openssl.cnf -days 365 -out certs/{userName}.pem";
            var convertToCrt = $"openssl x509 -in certs/{userName}.pem -out certs/{userName}.crt";
            RunCommand(makeKey);
            RunCommand(makeReq);
            RunCommand(signCert);
            RunCommand(convertToCrt);

            var fullPath = Path.GetFullPath($"certs/{userName}.crt");
            GeneratedCertPath.Background = Brushes.Coral;
            GeneratedCertPath.Text = fullPath;

            Directory.SetCurrentDirectory(oldDirectory);
        }

        public static void RunCommand(string command)
        {
            using (var myProcess = Process.Start("CMD.exe", "/K " + command))
            {
                do
                {
                    if (!myProcess.HasExited) myProcess.Refresh();
                } while (!myProcess.WaitForExit(1000));
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            File.Delete(Directory.GetCurrentDirectory() + "/../../CurrentUser/private.key");
            Process.GetCurrentProcess().Kill();
            base.OnClosing(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _previousWindow.Show();
            Hide();
        }
    }
}