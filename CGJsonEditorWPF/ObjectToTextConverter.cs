using CG.Test.Editor.Models.Nodes;
using System.Globalization;
using System.Windows.Data;

namespace CG.Test.Editor
{
    public class ObjectToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var node = (EditorObject)value;
            //var type = node.Type;

            //if (node.Nodes.Count > 10)
            //{
            //    if (node.Nodes.TryGetValue("name", out var foundNode) && foundNode is EditorValue foundValue && foundValue.Value is string name)//if (nodes.TryGetValue("name", out var nameNode) && nameNode is EditorValue nameValue && nameValue.Value is string name)
            //    {
            //        return $"{type} {{{name}...}}";
            //    }
            //    return $"{type} {{...}}";
            //}

            //return $"{type} {{{string.Join(", ", node.Nodes.Values)}}}";
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class ObjectToTextConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var name  = (string?)values[0]; 
    //        var nodes = (ObservableDictionary<string, EditorNode>)values[1];
    //        var type  = (EditorType)values[2];

    //        if (nodes.Count > 10)
    //        {
    //            if (name is not null)//if (nodes.TryGetValue("name", out var nameNode) && nameNode is EditorValue nameValue && nameValue.Value is string name)
    //            {
    //                return $"{type} {{{name}...}}";
    //            }
    //            return $"{type} {{...}}";
    //        }

    //        return $"{type} {{{string.Join(", ", nodes.Values)}}}";
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
