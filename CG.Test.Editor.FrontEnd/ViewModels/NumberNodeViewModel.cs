using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class NumberNodeViewModel : NodeViewModelBase
    {
        [ObservableProperty]
        private double _value;

        public NumberNodeViewModel(NodeViewModelBase? parent, SchemaNumberType type, double value) : base(parent)
        {
            Type  = type;
            Value = value;
        }

		public override SchemaNumberType Type { get; }

		public override NumberNodeViewModel Clone(NodeViewModelBase? parent) => new(parent, Type, Value);
    }
}
