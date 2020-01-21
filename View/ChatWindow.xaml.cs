using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CryptographyProject2019.Controller;
using CryptographyProject2019.Model;
using Org.BouncyCastle.Security;
using Path = System.IO.Path;

namespace CryptographyProject2019.View
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private Window _previousWindow;
        private readonly FileSystemWatcher _fileSystemWatcherMessages = new FileSystemWatcher();
        private readonly FileSystemWatcher _fileSystemWatcherClose = new FileSystemWatcher();
        private Account _receiver;
        private bool _close;
        private object locker = new object();
        private int rows = 0;
        private Grid messagesGrid;

        public ChatWindow()
        {
            InitializeComponent();
        }

        public ChatWindow Initialize(Window previousWindow, Account receiver)
        {
            _previousWindow = previousWindow;
            _receiver = receiver;
            Title.Text = AccountsController.GetInstance().CurrentAccount.Username + " <---> " + receiver.Username;
            string pathMessages = Directory.GetCurrentDirectory() + $"/../../ReceivedMessages/{AccountsController.GetInstance().CurrentAccount.Username}/";
            if (!Directory.Exists(pathMessages))
                Directory.CreateDirectory(pathMessages);
            string pathClose = Directory.GetCurrentDirectory() + $"/../../ChatRequests/";
            foreach (var enumerateFile in Directory.EnumerateFiles(pathMessages))
            {
                File.Delete(enumerateFile);
            }
            Task.Run(() => FSWatcherController.ExecuteWatcher(_fileSystemWatcherMessages, pathMessages, "*.*", DecryptMessage,
                () => _close));
            Task.Run(() => FSWatcherController.ExecuteWatcher(_fileSystemWatcherClose, pathClose, "*.sesreq", Close,
                () => _close));
            MakeGrid();
            MessageBlock.Text = receiver.Username;
            return this;
        }

        private void Close(object sender, FileSystemEventArgs e)
        {
            if (Path.GetFileNameWithoutExtension(e.Name) != AccountsController.GetInstance().CurrentAccount.Username) return;
            Account receiverAcc = AccountsController.GetInstance().CurrentAccount;
            Account senderAcc =
                SessionController.ReadSessionRequest(receiverAcc, false, locker);

            if (senderAcc.Username != _receiver.Username)
            {
                return;
            }
            MessageBox.Show($"Session with {_receiver.Username} is over!", "Chat session", MessageBoxButton.OK, MessageBoxImage.Information);
            Hide();
            ClosingController.CloseApplication();
            base.Close();
        }

        // ReSharper disable once InconsistentNaming
        private void MakeGrid()
        {
            messagesGrid = new Grid();
            messagesGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25, GridUnitType.Star) });
            messagesGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25, GridUnitType.Star) });
            messagesGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25, GridUnitType.Star) });
            messagesGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25, GridUnitType.Star) });
            MessagesScrollViewer.Content = messagesGrid;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var result = MessageBox.Show($"Do you want to cancel chat session with {_receiver.Username}?", "Chat session", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Hide();
                SessionController.CreateSessionRequestResponse(AccountsController.GetInstance().CurrentAccount, _receiver,
                    false, locker);
                ClosingController.CloseApplication();
            }
            else
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private void PosaljiPoruku(object sender, RoutedEventArgs e)
        {
            string message = MessageBlock.Text;
            if (message == "")
            {
                MessageBox.Show("Unesite poruku pa pokušajte ponovo poslati!");
                return;
            }

            if (!EncryptController.EncryptFileAsync(message, _receiver, locker))
            {
                MessageBox.Show("Slanje poruke je neuspjesno!");
            }
            AddMessageUi(new Message(message, DateTime.Now, MessageProperty.Sent));
        }

        private void AddMessageUi(Message message)
        {
            messagesGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            TextBlock dateBlock = new TextBlock();
            TextBlock messageBlock = new TextBlock();
            dateBlock.Text = message.Time.ToLongTimeString();
            messageBlock.Text = message.TextMessage;
            messagesGrid.Children.Add(dateBlock);
            messagesGrid.Children.Add(messageBlock);
            if (message.MessageProperty == MessageProperty.Received)
            {
                dateBlock.Foreground = new SolidColorBrush(Colors.LightSkyBlue);
                messageBlock.Foreground = new SolidColorBrush(Colors.LightSkyBlue);
                dateBlock.TextAlignment = TextAlignment.Left;
                messageBlock.TextAlignment = TextAlignment.Left;
                Grid.SetColumn(dateBlock, 0);
                Grid.SetColumn(messageBlock, 1);
                Grid.SetColumnSpan(messageBlock, 3);
                Grid.SetRow(dateBlock, rows);
                Grid.SetRow(messageBlock, rows);
                rows++;
            }
            else
            {
                dateBlock.Foreground = new SolidColorBrush(Colors.White);
                messageBlock.Foreground = new SolidColorBrush(Colors.White);
                dateBlock.TextAlignment = TextAlignment.Right;
                messageBlock.TextAlignment = TextAlignment.Right;
                Grid.SetColumn(dateBlock, 3);
                Grid.SetColumn(messageBlock, 0);
                Grid.SetColumnSpan(messageBlock, 3);
                Grid.SetRow(dateBlock, rows);
                Grid.SetRow(messageBlock, rows);
                rows++;
            }
        }
        private void DecryptMessage(object e, FileSystemEventArgs fsea)
        {
            string ext = Path.GetExtension(fsea.Name);
            if (!".aes".Equals(ext, StringComparison.CurrentCultureIgnoreCase) &&
                !".rc2".Equals(ext, StringComparison.CurrentCultureIgnoreCase) &&
                !".des3".Equals(ext, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }
            string message;
            try
            {
                message = DecryptController.DecryptEncryptedFile(
                    DecryptController.EncryptedFileParametersParser(fsea.FullPath, locker));
            }
            catch (Exception exception)
            {
                MessageBox.Show("Message can't be decrypted! Exception : " + exception.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (message == null)
            {
                MessageBox.Show("Message can't be decrypted!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Dispatcher?.Invoke(() => AddMessageUi(new Message(message, DateTime.Now, MessageProperty.Received)));
        }
    }
}
