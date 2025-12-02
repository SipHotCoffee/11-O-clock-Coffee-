using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class BooleanNodeViewModel : NodeViewModelBase
	{
        [ObservableProperty]
        private bool _value;

        public BooleanNodeViewModel(NodeViewModelBase? parent, SchemaBooleanType type, bool value) : base(parent)
        {
            Type = type;

            Value = value;
        }

        public override SchemaBooleanType Type { get; }

        public override BooleanNodeViewModel Clone(NodeViewModelBase? parent) => new(parent, Type, Value);
    }
}
