using CG.Test.Editor.Models.Nodes;
using System.Globalization;
using System.Windows.Data;

namespace CG.Test.Editor
{
    public class AnyNodeToTextConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return value switch
            //{
            //    EditorObject objectNode => new ObjectToTextConverter().Convert(objectNode, targetType, parameter, culture),
            //    EditorArray   arrayNode => new  ArrayToTextConverter().Convert(arrayNode , targetType, parameter, culture),
            //    EditorValue   valueNode => new  ValueToTextConverter().Convert([ valueNode.Value, valueNode.Type, valueNode.Tree ], targetType, parameter, culture),
            //    _                       => null,
            //};
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
