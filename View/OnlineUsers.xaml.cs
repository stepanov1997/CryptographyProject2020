using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CryptographyProject2019.Model;

namespace CryptographyProject2019.View
{
    /// <summary>
    /// Interaction logic for OnlineUsers.xaml
    /// </summary>
    public partial class OnlineUsers : UserControl
    {
        private bool _detailed;
        public int NumberOfRows { get; set; } = 1;
        public Grid GridView { get; set; }

        private Border[] _borders;

        public Dictionary<string, Button> UsernameButtonDictionary { get; } = new Dictionary<string, Button>();

        public OnlineUsers()
        {
            InitializeComponent();
        }

        public OnlineUsers Initialize(bool detailed)
        {
            _detailed = detailed;
            GridView = detailed ? MakeDetailedTable() : MakeSimpleTable();
            ScrollViewer.Content = GridView;
            return this;
        }

        private Grid MakeSimpleTable()
        {
            Grid grid = new Grid { Width = 27 };
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40, GridUnitType.Pixel) });

            _borders = new[] {
                MakeBorderedText(grid, 0, 0, "Username")
            };

            return grid;
        }

        public void AddRow(Account account)
        {
            GridView.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

            var index = new TextBlock
            {
                Text = NumberOfRows.ToString(),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.DarkSlateGray
            };
            var border1 = new Border
            {
                Child = index,
                Background = new SolidColorBrush(Colors.Wheat),
                BorderThickness = new Thickness(3)
            };

            GridView.Children.Add(border1);
            Grid.SetColumn(border1, 0);
            Grid.SetRow(border1, NumberOfRows);

            if (_detailed)
            {
                var username = new TextBlock
                {
                    Text = account.Username,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.DarkSlateGray
                };
                var border2 = new Border
                {
                    Child = username,
                    Background = new SolidColorBrush(Colors.Wheat),
                    BorderThickness = new Thickness(3)
                };

                var dugme = new Button { Content = "---->" };

                UsernameButtonDictionary.Add(account.Username, dugme);

                GridView.Children.Add(border2);
                GridView.Children.Add(dugme);

                Grid.SetColumn(border2, 1);
                Grid.SetRow(border2, NumberOfRows);
                Grid.SetColumn(dugme, 2);
                Grid.SetRow(dugme, NumberOfRows);
            }
            NumberOfRows++;
        }

        public void Clear()
        {
            Dispatcher?.Invoke(() =>
            {
                GridView.Children.Clear();
                GridView.Children.Add(_borders[0]);
                if (!_detailed) return;
                GridView.Children.Add(_borders[1]);
                GridView.Children.Add(_borders[2]);
            });
            UsernameButtonDictionary.Clear();
            NumberOfRows = 1;
        }

        private Grid MakeDetailedTable()
        {
            Grid grid = new Grid { Width = 500 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(79, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(254, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(167, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });

            _borders = new[]
            {
                MakeBorderedText(grid, 0, 0, "Number"),
                MakeBorderedText(grid, 1, 0, "Username"),
                MakeBorderedText(grid, 2, 0, "Open chat")
            };

            return grid;
        }

        public Border MakeBorderedText(Grid grid, int column, int row, string text)
        {
            Border border = new Border
            {
                BorderThickness = new Thickness(3),
                Background = new SolidColorBrush(Colors.DarkSlateGray),
                Child = new TextBlock
                {
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = text,
                    Foreground = new SolidColorBrush(Colors.White)
                }
            };
            grid.Children.Add(border);
            Grid.SetColumn(border, column);
            Grid.SetRow(border, row);

            return border;
        }
    }
}
