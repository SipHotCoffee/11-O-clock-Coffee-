using CG.Test.Editor.Models;
using CG.Test.Editor.Models.Types;
using System.Globalization;
using System.Windows.Data;

namespace CG.Test.Editor
{
    public class ValueToTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //var value =             values[0];
            //var type  = (EditorType)values[1];
            //var tree  = (EditorTree)values[2];

            //if (type is EditorEnumType enumType && value is int number)
            //{
            //    foreach (var member in enumType.Members)
            //    {
            //        if (member.Value == number)
            //        {
            //            return member.Name;
            //        }
            //    }
            //}
            //else if (type is EditorReferenceType && value is ulong id)
            //{
            //    var objectString = tree.TryGetObject(id, out var result) ? result.ToString() : "(null)";
            //    return $"Reference -> {objectString}";
            //}

            //if (value is string stringValue)
            //{
            //    return $"\"{stringValue}\"";
            //}

            //return value.ToString() ?? string.Empty;
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
