using System.Globalization;
using System.Windows.Data;

namespace CG.Test.Editor
{
    public class HasAnyItemsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flip = (bool)parameter;
            return (int)value == 0 == flip;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
