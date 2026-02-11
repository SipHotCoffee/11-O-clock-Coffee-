using CG.Test.Editor.FrontEnd;
using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public class ObjectNodeViewModel : NodeViewModelBase
    {
		public ObjectNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, LinkedSchemaObjectType type) : base(editor, parent)
		{
			Type = type;

            Nodes = [];
            Nodes.CollectionChanged += Nodes_CollectionChanged;
		}

        private void Nodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Editor.HasChanges = true;
		}

        public ObservableCollection<KeyValuePair<string, NodeViewModelBase>> Nodes { get; }

        public override LinkedSchemaObjectType Type { get; }

        public override ObjectNodeViewModel Clone(NodeViewModelBase? parent)
        {
            var result = new ObjectNodeViewModel(Editor, parent, Type);

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

		public override void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, ulong> referencedNodes)
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
