using CG.Test.Editor.FrontEnd;
using CG.Test.Editor.FrontEnd.Models.Types;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public class ObjectNodeViewModel : NodeViewModelBase
    {
		public ObjectNodeViewModel(NodeTree tree, NodeViewModelBase? parent, SchemaObjectType type) : base(tree, parent)
		{
			Type = type;

            Nodes = [];
            Nodes.CollectionChanged += Nodes_CollectionChanged;
		}

        private void Nodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Tree.Editor?.HasChanges = true;

			if (e.OldItems is not null)
			{
				foreach (var (_, removedNode) in e.OldItems.OfType<KeyValuePair<string, NodeViewModelBase>>())
				{
					removedNode.Release();
				}
			}
		}

        public ObservableCollection<KeyValuePair<string, NodeViewModelBase>> Nodes { get; }

        public override SchemaObjectType Type { get; }

        public override ObjectNodeViewModel Clone(NodeViewModelBase? parent)
        {
            var result = new ObjectNodeViewModel(Tree, parent, Type);

			foreach (var property in Type.Properties)
            {
				result.Nodes.Add(new KeyValuePair<string, NodeViewModelBase>(property.Name, Nodes[property.Index].Value.Clone(result)));
            }

            return result;
		}

        protected override string GetName(NodeViewModelBase item)
        {
            if (Type.TryGetProperty("name", out var property) && Nodes[property.Index].Value is StringNodeViewModel stringNode)
            {
                return stringNode.Value;
            }

            foreach (var pair in Nodes)
            {
                if (pair.Value == item)
                {
                    return pair.Key;
                }
            }
			return "Child not found!";
		}

        public override IEnumerable<NodeViewModelBase> Children => Nodes.Select((pair) => pair.Value);

		public override void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, int> referencedNodes)
		{
			writer.WriteStartObject();
			foreach (var pair in Nodes)
			{
				writer.WritePropertyName(pair.Key);
                pair.Value.SerializeTo(writer, referencedNodes);
			}
			writer.WriteEndObject();
		}

    }
}
