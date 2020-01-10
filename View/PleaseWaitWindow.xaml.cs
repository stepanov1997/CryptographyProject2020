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
        private Window _previousWindow;
        private object _locker;
        private bool _close2;

        public PleaseWaitWindow()
        {
            InitializeComponent();
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
            Dispatcher?.Invoke(() => chatWindow = new ChatWindow().Initialize(_previousWindow, receiver));

            _close2 = true;
            //_fileSystemWatcher.EnableRaisingEvents = false;
            //_fileSystemWatcher.Dispose();
            Dispatcher?.Invoke(delegate ()
            {
                chatWindow?.Show();
                _previousWindow.Hide();
                Hide();
            });
        }


        // 1. post session request
        public PleaseWaitWindow Initialize(Account sender, Account receiver, Window menuWindow, object locker)
        {
            _previousWindow = menuWindow;
            _locker = locker;
            SessionController.CreateSessionRequestResponse(sender, receiver, false, _locker);
            Task.Run(async () =>
            {
                string text = "";
                if (Dispatcher != null)
                {
                    await Dispatcher?.InvokeAsync(() => text = ConnectionBlock.Text);
                    var i = 30;
                    while (i > 0 && !_close2)
                    {
                        var secs = i;
                        // ReSharper disable once PossibleNullReferenceException
                        await Dispatcher.InvokeAsync(() => ConnectionBlock.Text = text + secs + "sec");
                        await Task.Delay(1000);
                        i--;
                    }
                    _close2 = true;
                    //_fileSystemWatcher.Dispose();
                    Hide();
                }
            });
            Task.Run(() => FSWatcherController.ExecuteWatcher(_fileSystemWatcher, Directory.GetCurrentDirectory() + @"\..\..\ChatRequests\", "*.sesres", ResponseCallback, ref _close));
            return this;
        }
    }
}
