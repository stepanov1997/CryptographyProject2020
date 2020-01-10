using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using CryptographyProject2019.Controller;
using CryptographyProject2019.Model;

namespace CryptographyProject2019.View
{
    /// <summary>
    ///     Interaction logic for MenuWindow.xaml
    /// </summary>
    public partial class MenuWindow : Window
    {
        private readonly Window _previousWindow;
        private object _locker = new object();
        private bool _closeOnline = false;
        private readonly FileSystemWatcher _fileSystemWatcherOnline = new FileSystemWatcher();
        private bool _closeRequest = false;
        private readonly FileSystemWatcher _fileSystemWatcherRequest = new FileSystemWatcher();
        private readonly OnlineUsers _onlineUsers;
        private static readonly string USER_REQUEST_PATH = USER_REQUEST_PATH = Directory.GetCurrentDirectory() + $@"\..\..\ChatRequests\{AccountsController.GetInstance().CurrentAccount.Username}.sesreq";

        public MenuWindow(Window previousWindow)
        {
            _previousWindow = previousWindow;
            InitializeComponent();
            Grid.Children.Add(_onlineUsers = new OnlineUsers().Initialize(true));
            _onlineUsers.Margin = new Thickness(5, 123, 4.333, 77.334);
            ImeLabel.Content = ImeLabel.Content + Environment.NewLine +
                               AccountsController.GetInstance().CurrentAccount.Username;
            string pathOnline = Directory.GetCurrentDirectory() + @"\..\..\CurrentUsers\";
            string pathRequest = Directory.GetCurrentDirectory() + @"\..\..\ChatRequests\";
            Task.Run(() => FSWatcherController.ExecuteWatcher(_fileSystemWatcherOnline, pathOnline, "*.key", AddOnlineAccounts, ref _closeOnline));
            Task.Run(() => FSWatcherController.ExecuteWatcher(_fileSystemWatcherRequest, pathRequest, "*.sesreq", ChatRequestIsHere, ref _closeRequest));
            AddOnlineAccounts(null, null);
        }

        // 2. Get session request
        // 3. Post session response
        private void ChatRequestIsHere(object o, FileSystemEventArgs e)
        {
            if (Path.GetFileNameWithoutExtension(e.Name) != AccountsController.GetInstance().CurrentAccount.Username) return; Account receiver = AccountsController.GetInstance().CurrentAccount;
            Account sender =
                SessionController.ReadSessionRequest(AccountsController.GetInstance().CurrentAccount, false, _locker);

            if (sender == null)
            {
                return;
            }

            var result = MessageBox.Show($"Do you want to chat with {sender.Username}?", "Chat request",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            if (!SessionController.CreateSessionRequestResponse(receiver, sender, true, _locker)) return;
            _closeRequest = true;
            _closeOnline = true;
            _fileSystemWatcherOnline.EnableRaisingEvents = false;
            _fileSystemWatcherOnline.Dispose();
            _fileSystemWatcherRequest.EnableRaisingEvents = false;
            _fileSystemWatcherRequest.Dispose();
            lock (_locker)
            {
                while (true)
                {
                    try
                    {
                        File.Delete(USER_REQUEST_PATH);
                        break;
                    }
                    catch (IOException)
                    {
                        Task.Delay(1000);
                    }
                }
            }
            ChatWindow chatWindow = null;
            if (Dispatcher != null)
            {
                Dispatcher.Invoke(() => chatWindow = new ChatWindow().Initialize(this, sender));
                Task.Delay(500);
                Dispatcher.Invoke(() => chatWindow.Show());
            }
            Dispatcher?.Invoke(Hide);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var mw = new MainWindow();
            mw.Show();
            Hide();
        }

        private void FileEncryptionClick(object sender, RoutedEventArgs e)
        {
            var few = new FileEncryptionWindow(this);
            few.Show();
            Hide();
        }

        private void AddOnlineAccounts(object e, FileSystemEventArgs fsea)
        {
            AccountsController.GetInstance().DeSerializeNow();
            _onlineUsers.Clear();
            if (Dispatcher == null) return;
            Dispatcher.Invoke(() =>
                AccountsController.GetInstance().ReadOnlineAccounts().ForEach(_onlineUsers.AddRow));
            foreach (var pair in _onlineUsers.UsernameButtonDictionary)
            {
                pair.Value.Click += (_, args) =>
                {
                    _closeOnline = true;
                    Account sender = AccountsController.GetInstance().CurrentAccount;
                    Account receiver = AccountsController.GetInstance().Accounts[pair.Key];
                    PleaseWaitWindow pleaseWaitWindow = new PleaseWaitWindow().Initialize(sender, receiver, this, _locker);
                    pleaseWaitWindow.ShowDialog();
                };
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ClosingController.CloseApplication();
            base.OnClosing(e);
        }

        private void FileDecryptionValidationClick(object sender, RoutedEventArgs e)
        {
            var fdv = new FileDecryptionValidationWindow(this);
            fdv.Show();
            Hide();
        }
    }
}