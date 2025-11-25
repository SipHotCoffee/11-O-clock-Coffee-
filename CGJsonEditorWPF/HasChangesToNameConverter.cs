using System.Globalization;
using System.Windows.Data;

namespace CG.Test.Editor.ViewModels
{
    public class HasChangesToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hasChanges = (bool)value;
            return hasChanges ? "Cancel" : "Close";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
