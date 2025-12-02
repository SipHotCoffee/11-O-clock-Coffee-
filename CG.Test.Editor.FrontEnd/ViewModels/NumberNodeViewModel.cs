using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class NumberNodeViewModel : NodeViewModelBase
    {
        [ObservableProperty]
        private double _value;

        public NumberNodeViewModel(SchemaNumberType type, double value)
        {
            Type  = type;
            Value = value;
        }

		public override SchemaNumberType Type { get; }

		public override NumberNodeViewModel Clone() => new(Type, Value);
    }
}
