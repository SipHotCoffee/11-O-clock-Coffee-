using CG.Test.Editor.FrontEnd.Models;
using System.Linq;

namespace CG.Test.Editor.FrontEnd.ViewModels
{

    public class ObjectNodeViewModel : NodeViewModelBase
    {
        public KeyValuePair<string, NodeViewModelBase>[] Nodes { get; }

		public ObjectNodeViewModel(SchemaObjectType type)
        {
			Type = type;

			var nodeVisitor = new NodeViewModelGenerator();
			Nodes = [.. type.Properties.Values.Select((property) => new KeyValuePair<string, NodeViewModelBase>(property.Name, property.Type.Visit(nodeVisitor)))];
		}


		public ObjectNodeViewModel(SchemaObjectType type, NodeViewModelBase[] nodes)
        {
            Type = type;
            Nodes = [.. type.Properties.Values.Select((property) => new KeyValuePair<string, NodeViewModelBase>(property.Name, nodes[property.Index]))];
        }

		public override SchemaObjectType Type { get; }

		public override ObjectNodeViewModel Clone()
        {
            var nodes = new NodeViewModelBase[Type.Properties.Count];
            foreach (var property in Type.Properties.Values)
            {
                nodes[property.Index] = Nodes[property.Index].Value.Clone();
            }
            return new ObjectNodeViewModel(Type, nodes);
		}
    }
}
