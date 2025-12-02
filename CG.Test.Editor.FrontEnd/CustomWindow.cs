using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace CG.Test.Editor.FrontEnd
{
    public class CustomWindow : Window
    {
       private static RenderTargetBitmap CreateResizedImage(ImageSource source, int width, int height)
        {
            var rect = new Rect(0, 0, width, height);

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(source, rect);
            }

            var resizedImage = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default);
            resizedImage.Render(drawingVisual);
            return resizedImage;
        }

        public static readonly DependencyProperty IsMicaBackgroundEnabledProperty = DependencyProperty.Register(nameof(IsMicaBackgroundEnabled), typeof(bool), typeof(CustomWindow));

        public static readonly DependencyProperty MinimizeButtonVisibilityProperty = DependencyProperty.Register(nameof(MinimizeButtonVisibility), typeof(Visibility), typeof(CustomWindow));
        public static readonly DependencyProperty MaximizeButtonVisibilityProperty = DependencyProperty.Register(nameof(MaximizeButtonVisibility), typeof(Visibility), typeof(CustomWindow));

        public static readonly DependencyProperty IsCloseButtonEnabledProperty = DependencyProperty.Register(nameof(IsCloseButtonEnabled), typeof(bool), typeof(CustomWindow));

        public static readonly DependencyProperty IsHorizontallyResizableProperty = DependencyProperty.Register(nameof(IsHorizontallyResizable), typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty   IsVerticallyResizableProperty = DependencyProperty.Register(nameof(IsVerticallyResizable), typeof(bool), typeof(CustomWindow), new PropertyMetadata(true));

        private static readonly ImageSource? _blurredBackground;

        private readonly ImageBrush? _backgroundBrush;

        private const int WM_SYSCOMMAND = 0x112;
        private HwndSource? _hwndSource;

        static CustomWindow()
        {
            using var key = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop");
            if (key is not null)
            {
                var wallpaperPath = (string)key.GetValue("Wallpaper")!;

                var wallpaper = new BitmapImage(new Uri(wallpaperPath))
                {
                    DecodePixelWidth  = 1000,
                    DecodePixelHeight = 1000
                };

                var blurEffect = new BlurEffect()
                {
                    Radius        = 100,
                    KernelType    = KernelType.Gaussian,
                    RenderingBias = RenderingBias.Performance,
                };

                var image = new Image()
                {
                    Source  = CreateResizedImage(wallpaper, 20, 20),
                    Effect  = blurEffect,
                    Stretch = Stretch.Fill,
                };

                image.Measure(new Size(SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height));
                image.Arrange(new Rect(new Size(SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height)));


                var renderTargetBitmap = new RenderTargetBitmap(
                        wallpaper.PixelWidth, wallpaper.PixelHeight,
                        wallpaper.DpiX, wallpaper.DpiY, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(image);

                _blurredBackground = renderTargetBitmap;
            }
        }

        public CustomWindow()
        {
            MaxHeight = SystemParameters.WorkArea.Height;

            IsCloseButtonEnabled = true;

            SourceInitialized += Window_SourceInitialized;

            _backgroundBrush = new ImageBrush(_blurredBackground)
            {
                TileMode      = TileMode.None,
                Stretch       = Stretch.Fill,
                AlignmentX    = AlignmentX.Left,
                AlignmentY    = AlignmentY.Top,
                ViewportUnits = BrushMappingMode.Absolute,
                ViewboxUnits  = BrushMappingMode.Absolute,
            };

            LocationChanged += OnMoveOrResize;
                SizeChanged += OnMoveOrResize;
               StateChanged += OnMoveOrResize;

            AllowsTransparency = true;
        }

        public bool IsMicaBackgroundEnabled
        {
            get => (bool)GetValue(IsMicaBackgroundEnabledProperty);
            set => SetValue(IsMicaBackgroundEnabledProperty, value);
        }

        public Visibility MinimizeButtonVisibility
        {
            get => (Visibility)GetValue(MinimizeButtonVisibilityProperty);
            set => SetValue(MinimizeButtonVisibilityProperty, value);
        }

        public Visibility MaximizeButtonVisibility
        {
            get => (Visibility)GetValue(MaximizeButtonVisibilityProperty);
            set => SetValue(MaximizeButtonVisibilityProperty, value);
        }

        public bool IsCloseButtonEnabled
        {
            get => (bool)GetValue(IsCloseButtonEnabledProperty);
            set => SetValue(IsCloseButtonEnabledProperty, value);
        }

        public bool IsHorizontallyResizable
        {
            get => (bool)GetValue(IsHorizontallyResizableProperty);
            set => SetValue(IsHorizontallyResizableProperty, value);
        }

        public bool IsVerticallyResizable
        {
            get => (bool)GetValue(IsVerticallyResizableProperty);
            set => SetValue(IsVerticallyResizableProperty, value);
        }


        private void OnMoveOrResize(object? sender, EventArgs e)
        {
            if (_backgroundBrush is not null)
            {
                _backgroundBrush.Viewbox = new Rect(0, 0, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);

                var left = Left % SystemParameters.WorkArea.Width;
                var top  = Top % SystemParameters.WorkArea.Height;

                if (WindowState == WindowState.Maximized)
                {
                    left = 0;
                    top = 0;
                }
                _backgroundBrush.Viewport = new Rect(-left, -top, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
            }
        }

        private enum ResizeDirection
        {
            Left        = 61441,
            Right       = 61442,
            Top         = 61443,
            TopLeft     = 61444,
            TopRight    = 61445,
            Bottom      = 61446,
            BottomLeft  = 61447,
            BottomRight = 61448,
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var imageBorder = (Border)Template.FindName("ImageBorder", this);
            imageBorder.Background = _backgroundBrush;

            var resizeN = (Border)Template.FindName("ResizeN", this);
            var resizeE = (Border)Template.FindName("ResizeE", this);
            var resizeS = (Border)Template.FindName("ResizeS", this);
            var resizeW = (Border)Template.FindName("ResizeW", this);

            var resizeNW = (Border)Template.FindName("ResizeNW", this);
            var resizeNE = (Border)Template.FindName("ResizeNE", this);
            var resizeSW = (Border)Template.FindName("ResizeSW", this);
            var resizeSE = (Border)Template.FindName("ResizeSE", this);

            var titleBar = (Border)Template.FindName("TitleBar", this);

            resizeN.MouseDown += Border_MouseDown;
            resizeE.MouseDown += Border_MouseDown;
            resizeS.MouseDown += Border_MouseDown;
            resizeW.MouseDown += Border_MouseDown;

            resizeNW.MouseDown += Border_MouseDown;
            resizeNE.MouseDown += Border_MouseDown;
            resizeSW.MouseDown += Border_MouseDown;
            resizeSE.MouseDown += Border_MouseDown;

            titleBar.MouseDown += TitleBar_MouseDown;

            var minimizeButton = (Button)Template.FindName("MinimizeButton", this);
            var maximizeButton = (Button)Template.FindName("MaximizeButton", this);
            var    closeButton = (Button)Template.FindName(   "CloseButton", this);

            var minimizeButtonVisibiltyBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath(MinimizeButtonVisibilityProperty),
            };

            minimizeButton.SetBinding(VisibilityProperty, minimizeButtonVisibiltyBinding);

            var maximizeButtonVisibiltyBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath(MaximizeButtonVisibilityProperty),
            };

            maximizeButton.SetBinding(VisibilityProperty, maximizeButtonVisibiltyBinding);

            var closeButtonEnabledBinding = new Binding()
            {
                Source = this,
                Path = new PropertyPath(IsCloseButtonEnabledProperty),
            };

            closeButton.SetBinding(IsEnabledProperty, closeButtonEnabledBinding);

            minimizeButton.Click += MinimizeButton_Click;
            maximizeButton.Click += MaximizeButton_Click;
               closeButton.Click +=    CloseButton_Click;

            switch (WindowState)
            {
                case WindowState.Maximized:
                    maximizeButton.Content = "\uE923";
                    break;
                case WindowState.Normal:
                    maximizeButton.Content = "\uE922";
                    break;
            }
        }

        private void Window_SourceInitialized(object? sender, EventArgs e)
        {
            if (sender is Visual visual)
            {
                _hwndSource = PresentationSource.FromVisual(visual) as HwndSource;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            var maximizeButton = (Button)sender;

            switch (WindowState)
            {
                case WindowState.Maximized:
                    maximizeButton.Content = "\uE922";
                    WindowState = WindowState.Normal;
                    break;
                case WindowState.Normal:
                    maximizeButton.Content = "\uE923";
                    WindowState = WindowState.Maximized;
                    break;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private const double AERO_OFFSET = 10;

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();

                var point = Extensions.GetMousePosition();

                if (point.X >= SystemParameters.WorkArea.Width - AERO_OFFSET && point.X <= SystemParameters.WorkArea.Width)
                {
                    Left = SystemParameters.WorkArea.Width * 0.5;
                    Top  = 0.0;

                    Width  = SystemParameters.WorkArea.Width * 0.5;
                    Height = SystemParameters.WorkArea.Height;
                }
                else if (point.X <= AERO_OFFSET && point.X >= 0)
                {
                    Left = 0.0;
                    Top  = 0.0;

                    Width  = SystemParameters.WorkArea.Width * 0.5;
                    Height = SystemParameters.WorkArea.Height;
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

        private void ResizeWindow(ResizeDirection direction)
        {
            var oldSizeToContentValue = SizeToContent;
            SendMessage(_hwndSource!.Handle, WM_SYSCOMMAND, (nint)direction, nint.Zero);
            SizeToContent = oldSizeToContentValue;
        }

        private void Border_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Border border && WindowState != WindowState.Maximized)
            {
                switch (border.Name)
                {
                    case "ResizeN":
                        if (IsVerticallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Top);
                        }
                        break;
                    case "ResizeE":
                        if (IsHorizontallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Right);
                        }
                        break;
                    case "ResizeS":
                        if (IsVerticallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Bottom);
                        }
                        break;
                    case "ResizeW":
                        if (IsHorizontallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Left);
                        }
                        break;
                    case "ResizeNW":
                        if (IsHorizontallyResizable && !IsVerticallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Left);
                        }
                        else if (!IsHorizontallyResizable && IsVerticallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Top);
                        }
                        else
                        {
                            ResizeWindow(ResizeDirection.TopLeft);
                        }
                        break;
                    case "ResizeNE":
                        if (IsHorizontallyResizable && !IsVerticallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Right);
                        }
                        else if (!IsHorizontallyResizable && IsVerticallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Top);
                        }
                        else
                        {
                            ResizeWindow(ResizeDirection.TopRight);
                        }
                        break;
                    case "ResizeSE":
                        if (IsHorizontallyResizable && !IsVerticallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Right);
                        }
                        else if (!IsHorizontallyResizable && IsVerticallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Bottom);
                        }
                        else
                        {
                            ResizeWindow(ResizeDirection.BottomRight);
                        }
                        break;
                    case "ResizeSW":
                        if (IsHorizontallyResizable && !IsVerticallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Left);
                        }
                        else if (!IsHorizontallyResizable && IsVerticallyResizable)
                        {
                            ResizeWindow(ResizeDirection.Bottom);
                        }
                        else
                        {
                            ResizeWindow(ResizeDirection.BottomLeft);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
