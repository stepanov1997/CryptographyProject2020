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
        private readonly FileSystemWatcher _fileSystemWatcher = new FileSystemWatcher();
        private OnlineUsers _onlineUsers;
        private Account _receiver;
        private bool _close;
        private object locker = new object();
        private int rows = 0;
        private Grid messagesGrid;

        public ChatWindow()
        {
            InitializeComponent();
        }

        private void AddOnlineAccounts(object e, FileSystemEventArgs fsea)
        {
            AccountsController.GetInstance().DeSerializeNow();
            Dispatcher?.Invoke(() =>
            {
                _onlineUsers.Clear();
                AccountsController.GetInstance().ReadOnlineAccounts().ForEach(_onlineUsers.AddRow);
            });
        }

        public ChatWindow Initialize(Window previousWindow, Account receiver)
        {
            _previousWindow = previousWindow;
            _receiver = receiver;
            OnlineUsersGrid.Children.Add(_onlineUsers = new OnlineUsers().Initialize(false));
            Title.Text = AccountsController.GetInstance().CurrentAccount.Username + " <---> " + receiver.Username;
            string path = Directory.GetCurrentDirectory() + @"/../../CurrentUsers/";
            string pathMessages = Directory.GetCurrentDirectory() + $"/../../ReceivedMessages/{AccountsController.GetInstance().CurrentAccount.Username}/";
            foreach (var enumerateFile in Directory.EnumerateFiles(pathMessages))
            {
                File.Delete(enumerateFile);
            }
            Task.Run(() => FSWatcherController.ExecuteWatcher(_fileSystemWatcher, path, "*.key", AddOnlineAccounts, ref _close));
            Task.Run(() => FSWatcherController.ExecuteWatcher(_fileSystemWatcher, pathMessages, "*.*", DecryptMessage, ref _close));
            MakeGrid();
            AddOnlineAccounts(null, null);
            MessageBlock.Text = receiver.Username;
            return this;
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
            ClosingController.CloseApplication();
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

            if (!EncryptController.EncryptFile(message, _receiver, locker))
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
            Dispatcher?.Invoke(()=>AddMessageUi(new Message(message, DateTime.Now, MessageProperty.Received)));
        }
    }
}
