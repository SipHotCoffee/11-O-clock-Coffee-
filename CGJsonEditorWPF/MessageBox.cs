using System.Windows;
using System.Windows.Controls;

namespace CG.Test.Editor
{
    public static class MessageBox
    {
        private static bool _isOpen = false;

        public static int ShowMessage(this Window owner, string message, string title = "", int defaultButtonIndex = 0, string first = "OK", params string[] buttons)
        {
            if (_isOpen)
            {
                return defaultButtonIndex;
            }

            var grid = new Grid()
            {
                Margin = new Thickness(2.5)
            };

            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.0, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            var messageLabel = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center,
                Text                = message,
                TextWrapping        = TextWrapping.Wrap,
                Margin              = new Thickness(10),
            };

            var contextMenu = new ContextMenu();
            var copyMenuItem = new MenuItem() { Header = "Copy" };
            copyMenuItem.Click += (sender, e) => Clipboard.SetText(message);

            contextMenu.Items.Add(copyMenuItem);

            var scrollViewer = new ScrollViewer()
            {
                HorizontalAlignment         = HorizontalAlignment.Stretch,
                VerticalAlignment           = VerticalAlignment.Stretch,
                Content                     = messageLabel,
                ContextMenu                 = contextMenu,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            Grid.SetRow(scrollViewer, 0);
            Grid.SetColumn(scrollViewer, 0);

            Grid.SetColumnSpan(scrollViewer, buttons.Length + 1);

            grid.Children.Add(scrollViewer);

            var result = 0;

            var window = new CustomWindow()
            {
                Owner                    = owner,
                Title                    = title,
                Content                  = grid,
                WindowStartupLocation    = WindowStartupLocation.CenterScreen,
                SizeToContent            = SizeToContent.Height,
                Width                    = 400,
                MaxHeight                = Math.Min(720, SystemParameters.FullPrimaryScreenHeight),
                ShowInTaskbar            = false,
                Topmost                  = true,
                MinimizeButtonVisibility = Visibility.Collapsed,
                MaximizeButtonVisibility = Visibility.Collapsed,
            };

            var firstButton = new Button()
            {
                IsDefault = defaultButtonIndex == 0,
                Content   = first,
                Margin    = new Thickness(2.5),
            };

            Grid.SetColumn(firstButton, 0);
            Grid.SetRow(firstButton, 1);

            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Star) });
            grid.Children.Add(firstButton);

            firstButton.Click += (sender, e) =>
            {
                result = 0;
                window.DialogResult = true;
                window.Close();
            };

            for (var i = 0; i < buttons.Length; i++)
            {
                var button = new Button()
                {
                    IsDefault = i + 1 == defaultButtonIndex,
                    Content   = buttons[i],
                    Margin    = new Thickness(2.5),
                };

                var buttonIndex = i + 1;
                button.Click += (sender, e) =>
                {
                    result = buttonIndex;
                    window.DialogResult = true;
                    window.Close();
                };

                Grid.SetColumn(button, i + 1);
                Grid.SetRow(button, 1);

                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Star) });
                grid.Children.Add(button);
            }

            _isOpen = true;
            window.ShowDialog();
            _isOpen = false;
            return result;
        }
    }
}