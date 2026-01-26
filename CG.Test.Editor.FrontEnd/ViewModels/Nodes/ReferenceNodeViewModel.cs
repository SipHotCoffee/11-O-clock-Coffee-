using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class ReferenceNodeViewModel : NodeViewModelBase
	{
		[ObservableProperty]
		private NodeViewModelBase? _node;

		[ObservableProperty]
		private NodePath? _path;

        public ReferenceNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, LinkedSchemaReferenceType type, NodeViewModelBase? node) : base(editor, parent)
        {
            Type = type;
			Node = node;

			if (node is not null)
			{
				editor.CachedPaths.AddReference(node);
			}
        }

		partial void OnNodeChanged(NodeViewModelBase? oldValue, NodeViewModelBase? newValue)
        {
            if (oldValue is not null)
			{
				Editor.CachedPaths.RemoveReference(oldValue);
			}

			if (newValue is not null)
			{
				Editor.CachedPaths.AddReference(newValue);
			}
        }

        public override LinkedSchemaReferenceType Type { get; }

		public override ReferenceNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Node);

		protected override string GetName(NodeViewModelBase item) => $"Reference -> ({(Node?.Name) ?? "null"})";

		public override void SerializeTo(Utf8JsonWriter writer)
		{
			writer.WriteNumberValue(Editor.CachedPaths.GetReferenceId(Node));
		}
	}
}
