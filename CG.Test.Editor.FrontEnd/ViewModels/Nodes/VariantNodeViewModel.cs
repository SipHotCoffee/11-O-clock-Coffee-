using CG.Test.Editor.FrontEnd.Models.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class VariantNodeViewModel : NodeViewModelBase
    {
		public SchemaVariantType VariantType { get; }

        public override SchemaObjectType Type => SelectedObject.Type;

		[ObservableProperty]
		private ObjectNodeViewModel _selectedObject;

        public VariantNodeViewModel(NodeTree tree, NodeViewModelBase? parent, SchemaVariantType type, ObjectNodeViewModel selectedObject) : base(tree, parent)
        {
			VariantType = type;

			SelectedObject = selectedObject;
        }

        public override IEnumerable<NodeViewModelBase> Children => SelectedObject.Children;

		public override void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, int> referencedNodes)
		   => SelectedObject.SerializeTo(writer, referencedNodes);

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

        public override VariantNodeViewModel Clone(NodeViewModelBase? parent) => new(Tree, parent, VariantType, SelectedObject.Clone(parent));
	}
}
