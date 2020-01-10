using System.ComponentModel;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CryptographyProject2019.Controller;
using CryptographyProject2019.Model;
using Microsoft.Win32;
using static System.Diagnostics.Process;

namespace CryptographyProject2019.View
{
    /// <summary>
    ///     Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private readonly Window _previousWindow;

        public RegisterWindow(Window previousWindow)
        {
            _previousWindow = previousWindow;
            InitializeComponent();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            fd.DefaultExt = ".pem";
            fd.Filter = "Digital certificate file (*.pem)|*.pem|Digital certificate file (*.crt)|*.crt";
            var old = Directory.GetCurrentDirectory();
            fd.InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../../CryptoFiles/certs/");
            if (fd.ShowDialog() != true) return;
            var sourceFile = fd.FileName;
            PathTextBox2.Text = sourceFile;
            PathTextBox2.ScrollToEnd();
        }

        private void RegisterAccountClick(object sender, RoutedEventArgs e)
        {
            var cert = new X509Certificate2();
            cert.Import(PathTextBox2.Text);
            if (!ValidateController.ValidateCertificates(cert))
            {
                MessageBox.Show("Certificate is revoked or signed with different CA!");
                return;
            }

            var account =
                AccountsController.GetInstance().AddAccount(new Account(Program.ReadName(cert), PasswordBox2.Password,
                    PathTextBox2.Text));

            if (account)
            {
                AccountsController.GetInstance().SerializeNow();
                MessageBox.Show("You successfully have made an account. Now you can login!");
                Task.Delay(2000);
                Hide();
                _previousWindow.Show();
            }
            else
            {
                MessageBox.Show("Please type all data in text boxes!");
            }

            ;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            File.Delete(Directory.GetCurrentDirectory() + "/../../CurrentUser/private.key");
            GetCurrentProcess().Kill();
            base.OnClosing(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _previousWindow.Show();
            Hide();
        }
    }
}