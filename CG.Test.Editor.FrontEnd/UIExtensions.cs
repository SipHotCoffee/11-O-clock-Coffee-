using System.Windows.Controls;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.FrontEnd
{
    public class MessageBoxParameters(string message, string title = "", string firstButton = "OK")
    {
        private readonly List<string> _buttons = [ firstButton ];

        public IReadOnlyList<string> Buttons => _buttons;

        public int DefaultButtonIndex { get; private set; } = 0;

        public string Message { get; } = message;

        public string Title { get; } = title;

        public void AddButton(string buttonLabel, bool isDefault = false)
        {
            if (isDefault)
            {
                DefaultButtonIndex = _buttons.Count;
            }

            _buttons.Add(buttonLabel);
        }
    }

    public static class UIExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        public static POINT GetMousePosition()
        {
            GetCursorPos(out var lpPoint);
            return lpPoint;
        }

        extension(Type type)
        {
            public string GetTypeName()
            {
                if (type.IsGenericType)
                {
                    return $"{type.Name[..^2]}<{string.Join(", ", type.GenericTypeArguments.Select(GetTypeName))}>";
                }

                return type.Name;
            }
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;

        private static void Window_SourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper((Window)sender!).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            _ = SetWindowLong(hwnd, GWL_STYLE, value & ~WS_MAXIMIZEBOX);
        }

        private static bool _isOpen = false;

        extension(Window ownerWindow)
        {
            public int ShowMessage(string message) => ownerWindow.ShowMessage(new MessageBoxParameters(message));

			public int ShowMessage(MessageBoxParameters parameters)
			{
				if (_isOpen)
                {
                    return parameters.DefaultButtonIndex;
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
                    Text                = parameters.Message,
                    TextWrapping        = TextWrapping.Wrap,
                    Margin              = new Thickness(10),
                };

                var contextMenu = new ContextMenu();
                var copyMenuItem = new MenuItem() { Header = "Copy" };
                copyMenuItem.Click += (sender, e) => Clipboard.SetText(parameters.Message);

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

                Grid.SetColumnSpan(scrollViewer, parameters.Buttons.Count);

                grid.Children.Add(scrollViewer);

                var result = 0;

                var owner = ownerWindow;
                if (!ownerWindow.IsLoaded)
                {
                    owner = null;
                }

                var window = new CustomWindow()
                {
                    Owner                 = owner,
                    Title                 = parameters.Title,
                    Content               = grid,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode            = ResizeMode.CanResize,
                    SizeToContent         = SizeToContent.Height,
                    Width                 = 400,
                    MaxHeight             = Math.Min(720, SystemParameters.FullPrimaryScreenHeight),
                    ShowInTaskbar         = false,
                    Topmost               = true
                };

                window.SourceInitialized += Window_SourceInitialized;

                //var firstButton = new Button()
                //{
                //    IsDefault = defaultButtonIndex == 0,
                //    Content   = firstButtonLabel,
                //    Margin    = new Thickness(2.5),
                //};

                //Grid.SetColumn(firstButton, 0);
                //Grid.SetRow(firstButton, 1);

                //grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Star) });
                //grid.Children.Add(firstButton);

                //firstButton.Click += (sender, e) =>
                //{
                //    result = 0;
                //    window.DialogResult = true;
                //    window.Close();
                //};

                for (var i = 0; i < parameters.Buttons.Count; i++)
                {
                    var button = new Button()
                    {
                        IsDefault = i == parameters.DefaultButtonIndex, //i + 1 == defaultButtonIndex,
                        Content   = parameters.Buttons[i],
                        Margin    = new Thickness(2.5),
                    };

                    var buttonIndex = i;

                    button.Click += (sender, e) =>
                    {
                        result = buttonIndex;
                        window.DialogResult = true;
                        window.Close();
                    };

                    Grid.SetColumn(button, i);
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
}
