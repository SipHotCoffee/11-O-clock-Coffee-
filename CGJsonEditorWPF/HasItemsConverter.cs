using System.Globalization;
using System.Windows.Data;

namespace CG.Test.Editor
{
    public class HasItemsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itemCount = (int)parameter;
            return (int)value == itemCount;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
