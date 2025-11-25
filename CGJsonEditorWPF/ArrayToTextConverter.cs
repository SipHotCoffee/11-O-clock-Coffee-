using CG.Test.Editor.Models.Nodes;
using CG.Test.Editor.Models.Types;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace CG.Test.Editor
{
    public class ArrayToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var arrayNode = (EditorArray)value;
            //var type      = (EditorGenericType)arrayNode.Type;

            //if (arrayNode.Elements.Count == 0 || arrayNode.Elements.Count > 10 || type.TypeArgument is not EditorValueType)
            //{
            //    return $"{type.TypeArgument}[{arrayNode.Elements.Count}]";
            //}

            //return $"{type.TypeArgument}[] {{ {string.Join(", ", arrayNode.Elements)} }}";
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class ArrayToTextConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var elements = (ObservableCollection<EditorNode>)values[0];
    //        var type     = (EditorGenericType)values[1];

    //        if (elements.Count == 0 || elements.Count > 10 || type.TypeArgument is not EditorValueType)
    //        {
    //            return $"{type.TypeArgument}[{elements.Count}]";
    //        }

    //        return $"{type.TypeArgument}[] {{ {string.Join(", ", elements)} }}";
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
