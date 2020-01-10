using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CryptographyProject2019.Controller;
using Microsoft.Win32;

namespace CryptographyProject2019.View
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Program.Run();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            fd.DefaultExt = ".pem";
            fd.Filter = "Digital certificate file (*.pem)|*.pem|Digital certificate file (*.crt)|*.crt";
            fd.InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + @"\..\..\CryptoFiles\certs\");
            if (fd.ShowDialog() != true) return;
            var sourceFile = fd.FileName;
            PathTextBox.Text = sourceFile;
            PathTextBox.ScrollToEnd();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var rw = new RegisterWindow(this);
            rw.Show();
            Hide();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            File.Delete(Directory.GetCurrentDirectory() + $"/../../CurrentUsers/{AccountsController.GetInstance().CurrentAccount.Username}.key");
            Process.GetCurrentProcess().Kill();
            base.OnClosing(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var account =
                AccountsController.GetInstance().GetAccount(UsernameTextBox.Text, PasswordBox.Password,
                    PathTextBox.Text);
            if (account == null)
            {
                MessageBox.Show("Username and password are not valid. Please try again.", "Unsuccessful login");
            }
            else
            {
                MessageBox.Show("Username and password are valid. Congratulations.", "Successful login");
                AccountsController.GetInstance().ChangeCurrentAccount(account);
                var mw = new MenuWindow(this);
                mw.Show();
                Hide();
            }
        }

        private void MakeCertificateClick(object sender, RoutedEventArgs e)
        {
            var mcw = new MakeCertificateWindow(this);
            mcw.Show();
            Hide();
        }
    }
}