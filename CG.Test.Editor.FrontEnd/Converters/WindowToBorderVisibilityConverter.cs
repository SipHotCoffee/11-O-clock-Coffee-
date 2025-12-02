using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CG.Test.Editor.FrontEnd.Converters
{
    public class WindowToBorderVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var windowState = (WindowState)value;
            if (windowState == WindowState.Maximized)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
