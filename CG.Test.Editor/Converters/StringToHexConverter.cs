using System.Globalization;
using System.Windows.Data;

namespace CG.Test.Editor.Converters
{
    public class StringToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var byteValue = (byte)value;
            return System.Convert.ToString(byteValue, 16).ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = (string)value;

            if (!int.TryParse(stringValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var intValue))
            {
                return (byte)0;
            }

            if (intValue > byte.MaxValue)
            {
                return byte.Parse(stringValue.AsSpan(0, 2), NumberStyles.HexNumber);
            }

            return (byte)intValue;
        }
    }
}
