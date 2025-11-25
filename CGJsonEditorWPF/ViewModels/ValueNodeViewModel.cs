using CG.Test.Editor.Json.Dialogs;
using CG.Test.Editor.Models.Nodes;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace CG.Test.Editor.ViewModels
{
    public partial class ValueNodeViewModel : NodeViewModelBase
    {
        public JsonValueNode ValueNode { get; }

        [ObservableProperty]
        private object? _value;

        public ValueNodeViewModel(NodeEditorViewModel editor, JsonValueNode node) : base(editor, node)
        {
            ValueNode = node;

            Value = node.Value;
        }

        public override bool Navigate(string name)
        {
            if (Value is bool boolValue)
            {
                Value = !boolValue;
            }
            else if (Value is string stringValue)
            {
                var viewModel = new StringValueDialogViewModel()
                {
                    Label = name,
                    Text  = stringValue
                };

                var dialog = new StringValueDialogView()
                {
                    DataContext = viewModel
                };

                if (dialog.ShowDialog() == true)
                {
                    Value = viewModel.Text;
                }
            }
            else if (Value is IConvertible convertible)
            {
                var dialog = new NumericValueDialog()
                {
                    Owner = Editor.OwnerWindow,

                    Minimum = convertible switch
                    {
                        byte   =>   byte.MinValue,
                        ushort => ushort.MinValue,
                        uint   =>   uint.MinValue,
                        ulong  =>  ulong.MinValue,

                        sbyte => sbyte.MinValue,
                        short => short.MinValue,
                        int   =>   int.MinValue,
                        long  =>  long.MinValue,

                        float  =>  float.MinValue,
                        double => double.MinValue,

                        _ => 0.0
                    },

                    Maximum = convertible switch
                    {
                        byte   =>   byte.MaxValue,
                        ushort => ushort.MaxValue,
                        uint   =>   uint.MaxValue,
                        ulong  =>  ulong.MaxValue,

                        sbyte => sbyte.MaxValue,
                        short => short.MaxValue,
                        int   =>   int.MaxValue,
                        long  =>  long.MaxValue,

                        float  =>  float.MaxValue,
                        double => double.MaxValue,

                        _ => 1.0
                    },

                    IsIntegral = convertible switch
                    {
                        byte   => true,
                        ushort => true,
                        uint   => true,
                        ulong  => true,

                        sbyte => true,
                        short => true,
                        int   => true,
                        long  => true,

                        float  => false,
                        double => false,

                        _ => false
                    },

                    Value = convertible.ToDouble(CultureInfo.CurrentCulture),
                };

                if (dialog.ShowDialog() == true)
                {
                    Value = Convert.ChangeType(dialog.Value, convertible.GetType());
                }
            }
            return false;
        }
    }
}
