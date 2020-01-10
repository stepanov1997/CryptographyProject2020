using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using CryptographyProject2019.Controller;
using Microsoft.Win32;

namespace CryptographyProject2019.View
{
    /// <summary>
    ///     Interaction logic for FileDecryptionValidationWindow.xaml
    /// </summary>
    public partial class FileDecryptionValidationWindow : Window
    {
        private static string _pathToDecryptedFile;
        private readonly Window _previousWindow;

        public FileDecryptionValidationWindow(Window previousWindow)
        {
            _previousWindow = previousWindow;
            InitializeComponent();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var pathToFile = FileButton.Content.ToString();

            try
            {
                //_pathToDecryptedFile = 
                //        DecryptController.DecryptEncryptedFile(DecryptController.EncryptedFileParametersParser(pathToFile));
            }
            catch (Exception _)
            {
                MessageBox.Show("File can't be decrypted!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_pathToDecryptedFile != "")
            {
                MessageBox.Show("Congratulations, you successfully decrypted a file!");
                DecryptButton.IsEnabled = false;
                FindFolderButton.IsEnabled = false;
                RunProgramButton.IsEnabled = false;
                new Thread(() =>
                {
                    Thread.Sleep(3000);
                    Dispatcher?.Invoke(() =>
                    {
                        FindFolderButton.IsEnabled = true;
                        RunProgramButton.IsEnabled = true;
                        return DecryptButton.IsEnabled = true;
                    });
                }).Start();
            }
            else
            {
                MessageBox.Show("File can't be decrypted!", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.GetDirectoryName(_pathToDecryptedFile)?.Replace(@"\\", @"\");
            Process.Start("explorer.exe", path);
        }

        private void RunProgramButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pathToDecryptedFile == "")
            {
                MessageBox.Show("File is not decrypted!");
                return;
            }

            var parentFolder = Path.GetDirectoryName(_pathToDecryptedFile);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(_pathToDecryptedFile);
            var fileExt = Path.GetExtension(_pathToDecryptedFile);

            string compileProgram;
            string runProgram;

            if (fileExt == ".c" || fileExt == ".cpp")
            {
                compileProgram =
                    $"gcc -o {parentFolder}/{fileNameWithoutExt}.exe {parentFolder}/{fileNameWithoutExt}{fileExt}";
                runProgram = $"{parentFolder}/{fileNameWithoutExt}.exe";
            }
            else if (fileExt == ".java")
            {
                compileProgram = $"javac {parentFolder}/{fileNameWithoutExt}{fileExt}";
                runProgram = $"java {parentFolder}/{fileNameWithoutExt}.java";
            }
            else
            {
                MessageBox.Show("It is not source code file!");
                return;
            }

            MakeCertificateWindow.RunCommand(compileProgram);
            MakeCertificateWindow.RunCommand(runProgram);
        }

        private void FileButton_Click_1(object sender, RoutedEventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            fd.DefaultExt = "*.c";
            fd.Filter = "AES Files (*.aes)|*.aes|RC2 (*.rc2)|*.rc2|Triple DES (*.des3)|*.des3";
            fd.InitialDirectory = Path.GetFullPath(Directory.GetCurrentDirectory() + "/../../ReceivedMessages/");
            if (fd.ShowDialog() != true) return;
            var sourceFile = fd.FileName;
            FileButton.Content = sourceFile;
        }
    }
}