using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using CryptographyProject2019.Controller;
using Microsoft.Win32;

namespace CryptographyProject2019.View
{
    /// <summary>
    ///     Interaction logic for FileEncryptionWindow.xaml
    /// </summary>
    public partial class FileEncryptionWindow : Window
    {
        private readonly Window _previousWindow;

        public FileEncryptionWindow(Window previousWindow)
        {
            _previousWindow = previousWindow;
            InitializeComponent();
            foreach (var accountsKey in AccountsController.GetInstance().Accounts.Keys)
                ReceiverComboBox.Items.Add(accountsKey);
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            fd.Filter = "Java Code Files (*.java)|*.java|C Code Files (*.c)|*.c|C++ Code Files (*.cpp)|*.cpp";
            var old = Directory.GetCurrentDirectory();
            fd.InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../../FilesForSending");
            if (fd.ShowDialog() != true) return;
            var sourceFile = fd.FileName;
            FileButton.Content = sourceFile;
        }

        private void BackClick(object sender, RoutedEventArgs e)
        {
            _previousWindow.Show();
            Hide();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            File.Delete(Directory.GetCurrentDirectory() + "/../../CurrentUser/private.key");
            Process.GetCurrentProcess().Kill();
            base.OnClosing(e);
        }

        private void EncryptClick(object sender, RoutedEventArgs e)
        {
            var path = FileButton.Content.ToString();
            var data = File.ReadAllBytes(path);

            var algs = CheckAlgorithm();
            if (algs.Item1 == null || algs.Item2 == null) return;
            var receiver = (string) ReceiverComboBox.SelectionBoxItem;
            var accReceiver = AccountsController.GetInstance().Accounts[receiver];
            //EncryptController.EncryptFile((path, data), accReceiver, algs.Item1, algs.Item2);
            MessageBox.Show("Congratulations, you successfully encrypted a file!");
            EncryptButton.IsEnabled = false;
            new Thread(() =>
            {
                Thread.Sleep(3000);
                Dispatcher?.Invoke(() => EncryptButton.IsEnabled = true);
            }).Start();
        }

        public (SymmetricAlgorithm, HashAlgorithm) CheckAlgorithm()
        {
            (SymmetricAlgorithm, HashAlgorithm) ret;
            if (Equals(SymmetricComboBox.SelectionBoxItem, AesItem.Content))
            {
                ret.Item1 = Aes.Create();
            }
            else if (Equals(SymmetricComboBox.SelectionBoxItem, Rc4Item.Content))
            {
                ret.Item1 = RC2.Create();
            }
            else if (Equals(SymmetricComboBox.SelectionBoxItem, DesItem.Content))
            {
                ret.Item1 = TripleDES.Create();
            }
            else
            {
                MessageBox.Show("Select symmetric algorithm!");
                ret.Item1 = null;
            }

            if (Equals(HashComboBox.SelectionBoxItem, SHA1Item.Content))
            {
                ret.Item2 = SHA1.Create();
            }
            else if (Equals(HashComboBox.SelectionBoxItem, SHA256Item.Content))
            {
                ret.Item2 = SHA256.Create();
            }
            else
            {
                MessageBox.Show("Select hash algorithm!");
                ret.Item2 = null;
            }

            return ret;
        }
    }
}