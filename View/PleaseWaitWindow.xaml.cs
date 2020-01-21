using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CryptographyProject2019.Controller;
using CryptographyProject2019.Model;
using Path = System.IO.Path;

namespace CryptographyProject2019.View
{
    /// <summary>
    /// Interaction logic for PleaseWaitWindow.xaml
    /// </summary>
    public partial class PleaseWaitWindow : Window
    {
        // ReSharper disable once InconsistentNaming
        private readonly string USER_RESPONSE_PATH = Directory.GetCurrentDirectory() +
                                                     $@"\..\..\ChatRequests\{AccountsController.GetInstance().CurrentAccount.Username}.sesres";

        private bool _close;
        private readonly FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();
        private Action _closeWatcher;
        private ChatRoomWindow _previousWindow;
        private object _locker;

        public PleaseWaitWindow()
        {
            InitializeComponent();
        }
        public PleaseWaitWindow(ChatRoomWindow chatRoomWindow) : this()
        {
            _previousWindow = chatRoomWindow;
        }

        // 4. get session response
        private void ResponseCallback(object sender, FileSystemEventArgs e)
        {
            if (Path.GetFileNameWithoutExtension(e.Name) !=
                AccountsController.GetInstance().CurrentAccount.Username) return;
            var receiver =
                SessionController.ReadSessionRequest(AccountsController.GetInstance().CurrentAccount, true, _locker);
            if (receiver == null) return;
            while (true)
            {
                try
                {
                    lock (_locker)
                    {
                        File.Delete(USER_RESPONSE_PATH);
                    }
                    break;
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                {
                    Task.Delay(500);
                }
            }

            ChatWindow chatWindow = null;
            _closeWatcher();
            _close = true;
            Dispatcher?.Invoke(() => chatWindow = new ChatWindow().Initialize(_previousWindow, receiver));

            Dispatcher?.Invoke(delegate ()
            {
                chatWindow.Show();
                _previousWindow.Hide();
                Hide();
            });
        }


        // 1. post session request
        public PleaseWaitWindow Initialize(Account sender, Account receiver, Action closeWatcher, object locker)
        {
            _closeWatcher = closeWatcher;
            _locker = locker;
            SessionController.CreateSessionRequestResponse(sender, receiver, false, _locker);
            Task.Run(async () =>
            {
                string text = "";
                if (Dispatcher != null)
                {
                    await Dispatcher?.InvokeAsync(() => text = ConnectionBlock.Text);
                    var i = 30;
                    while (i > 0 && !_close)
                    {
                        var secs = i;
                        // ReSharper disable once PossibleNullReferenceException
                        await Dispatcher.InvokeAsync(() => ConnectionBlock.Text = text + secs + "sec");
                        await Task.Delay(1000);
                        i--;
                    }
                    Close();
                }
                _closeWatcher();
            });
            Task.Run(() => FSWatcherController.ExecuteWatcher(_fileSystemWatcher, Directory.GetCurrentDirectory() + @"\..\..\ChatRequests\", "*.sesres", ResponseCallback, () => _close));
            return this;
        }
    }
}
