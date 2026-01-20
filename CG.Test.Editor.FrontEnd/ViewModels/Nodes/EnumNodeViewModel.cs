using CG.Test.Editor.FrontEnd.Models.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class EnumNodeViewModel : NodeViewModelBase
    {
        public override SchemaEnumType Type { get; }

        [ObservableProperty]
        private int _selectedIndex;

        [ObservableProperty]
        private string _selectedItem;

        public EnumNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, SchemaEnumType type, int selectedIndex) : base(editor, parent)
        {
            Type = type;

            SelectedItem = Type.PossibleValues[0];
            SelectedIndex = selectedIndex;
        }

        partial void OnSelectedIndexChanged(int oldValue, int newValue)
        {
            SelectedItem = Type.PossibleValues[newValue];
        }

        public override EnumNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, Parent, Type, SelectedIndex);

        public override void SerializeTo(Utf8JsonWriter writer)
        {
            writer.WriteStringValue(Type.PossibleValues[SelectedIndex]);
        }

        protected override string GetName(NodeViewModelBase item) => Type.PossibleValues[SelectedIndex];
	}
}
