using CG.Test.Editor.FrontEnd.Models;
using System.Collections.ObjectModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public class ObjectNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, SchemaObjectType type) : NodeViewModelBase(editor, parent)
    {
        public ObservableCollection<KeyValuePair<string, NodeViewModelBase>> Nodes { get; } = [];

        public override SchemaObjectType Type { get; } = type;

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
			return base.GetName(item);
		}
    }
}
