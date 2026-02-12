using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class ReferenceNodeViewModel : NodeViewModelBase
	{
		[ObservableProperty]
		private NodePath? _path;

		[ObservableProperty]
		private NodeViewModelBase? _node;

        public ReferenceNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, LinkedSchemaReferenceType type, NodeViewModelBase? node) : base(editor, parent)
        {
            Type = type;
			Node = node;
		}

        public override LinkedSchemaReferenceType Type { get; }

        public override ReferenceNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Node);

        protected override string GetName(NodeViewModelBase item) => $"Reference -> ({(Node?.Name) ?? "null"})";

        public override void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, ulong> referencedNodes)
		{
			if (Node is not null && Node.IsAlive && referencedNodes.TryGetValue(Node, out var id))
			{
				writer.WriteNumberValue(id);
			}
			else
			{
				writer.WriteNumberValue(0);
			}
		}
	}
}
