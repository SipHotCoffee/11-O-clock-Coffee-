using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class VariantNodeViewModel : NodeViewModelBase
    {
        public override LinkedSchemaVariantType Type { get; }

		[ObservableProperty]
		private ObjectNodeViewModel _selectedObject;

        public VariantNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, LinkedSchemaVariantType type, ObjectNodeViewModel selectedObject) : base(editor, parent)
        {
            Type = type;

			SelectedObject = selectedObject;
        }

        public override void SerializeTo(Utf8JsonWriter writer) => SelectedObject.SerializeTo(writer);

        protected override string GetName(NodeViewModelBase item)
        {
			if (SelectedObject.Type.TryGetProperty("name", out var property) && SelectedObject.Nodes[property.Index].Value is StringNodeViewModel stringNode)
			{
				return stringNode.Value;
			}

			foreach (var pair in SelectedObject.Nodes)
			{
				if (pair.Value == item)
				{
					return pair.Key;
				}
			}
			return "Child not found!";
		}

        public override VariantNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, SelectedObject.Clone(parent));
	}
}
