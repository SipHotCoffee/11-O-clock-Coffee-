using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class BooleanNodeViewModel : NodeViewModelBase
	{
        [ObservableProperty]
        private bool _value;

        public BooleanNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, SchemaBooleanType type, bool value) : base(editor, parent)
        {
            Type = type;

            Value = value;
        }

        public override SchemaBooleanType Type { get; }

        public override BooleanNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Value);

        protected override string GetName(NodeViewModelBase item) => Value.ToString();
    }
}
