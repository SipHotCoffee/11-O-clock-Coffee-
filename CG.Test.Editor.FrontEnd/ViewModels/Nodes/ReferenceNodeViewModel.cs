using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System.Xml.Linq;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class ReferenceNodeViewModel : NodeViewModelBase
	{
		private WeakReference<NodeViewModelBase>? _nodeReference;

		[ObservableProperty]
		private NodePath? _path;

        public ReferenceNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, LinkedSchemaReferenceType type, NodeViewModelBase? node) : base(editor, parent)
        {
			_nodeReference = node is not null ? new WeakReference<NodeViewModelBase>(node) : null;

			Type = type;
        }

		public NodeViewModelBase? Node
		{
			get
			{
				if (_nodeReference is not null && _nodeReference.TryGetTarget(out var node))
				{
					return node;
				}
				return null;
			}
			set
			{
				if (value is not null)
				{
					OnPropertyChanging();

					if (_nodeReference is null)
					{
						_nodeReference = new WeakReference<NodeViewModelBase>(value);
					}
					else
					{
						_nodeReference.SetTarget(value);
					}
					
					OnPropertyChanged();
				}
			}
		}

        public override LinkedSchemaReferenceType Type { get; }

        public override ReferenceNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Node);

        protected override string GetName(NodeViewModelBase item) => $"Reference -> ({(Node?.Name) ?? "null"})";

        public override void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, ulong> referencedNodes)
		{
			if (_nodeReference is not null && _nodeReference.TryGetTarget(out var node) && referencedNodes.TryGetValue(node, out var id))
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
