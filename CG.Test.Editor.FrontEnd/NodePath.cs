using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd
{
    public interface INodeIdentifier
    {
        void SerializeTo(Utf8JsonWriter writer);
    }

    public class IndexIdentifier(int elementIndex) : INodeIdentifier
	{
        public int ElementIndex { get; } = elementIndex;

        public void SerializeTo(Utf8JsonWriter writer) => writer.WriteNumberValue(ElementIndex);
    }

    public class NameIdentifier(string propertyName) : INodeIdentifier
	{
        public string PropertyName { get; } = propertyName;

        public void SerializeTo(Utf8JsonWriter writer) => writer.WriteStringValue(PropertyName);
    }

    public class NodePath : IEnumerable<INodeIdentifier>
    {
		public static NodePath Root { get; } = new NodePath([]);

		private readonly INodeIdentifier[] _path;

        private NodePath(IEnumerable<INodeIdentifier> path)
        {
            _path = [.. path];
        }

        public NodePath GetParent() { return new NodePath(_path.SkipLast(1)); }

		public NodePath GetChild(INodeIdentifier identifier) => new NodePath(_path.Append(identifier));

        private bool TryNavigate(NodeViewModelBase root, [NotNullWhen(true)] out NodeViewModelBase? node, int depth)
        {
            if (depth >= _path.Length)
            {
				node = root;
                return true;
            }

            var key = _path[depth];
            if (root is ArrayNodeViewModel arrayNode && key is IndexIdentifier index)
            {
                return TryNavigate(arrayNode.Elements[index.ElementIndex], out node, depth + 1);
            }
            else if (root is ObjectNodeViewModel objectNode && key is NameIdentifier name && objectNode.Type.TryGetProperty(name.PropertyName, out var property))
            {
                return TryNavigate(objectNode.Nodes[property.Index].Value, out node, depth + 1);
            }

            node = null;
            return false;
        }

        public bool TryNavigate(NodeViewModelBase root, [NotNullWhen(true)] out NodeViewModelBase? node) => TryNavigate(root, out node, 0);

        public override string ToString() => string.Join('/', _path);

        public IEnumerator<INodeIdentifier> GetEnumerator() => _path.OfType<INodeIdentifier>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
