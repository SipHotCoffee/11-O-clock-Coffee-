using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class NumberNodeViewModel : NodeViewModelBase
    {
        [ObservableProperty]
        private double _value;

        public NumberNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, SchemaNumberType type, double value) : base(editor, parent)
        {
            Type  = type;
            Value = value;
        }

		public override SchemaNumberType Type { get; }

		public override NumberNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Value);
    }
}
