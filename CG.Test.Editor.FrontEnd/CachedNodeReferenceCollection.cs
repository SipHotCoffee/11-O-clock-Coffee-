using CG.Test.Editor.FrontEnd.ViewModels;
using System.Collections;

namespace CG.Test.Editor.FrontEnd
{
    public class CachedNodeReferenceCollection : IReadOnlyCollection<KeyValuePair<NodeViewModelBase, ulong>>
    {
		private class NodeReference(CachedNodeReferenceCollection collection)
        {
            public ulong Id { get; } = collection._nextId++;

            public int ReferenceCount { get; set; } = 0;
        }

        private readonly Dictionary<NodeViewModelBase, NodeReference> _cachedNodeReferences = [];

        private ulong _nextId = 1;

        public ulong GetReferenceId(NodeViewModelBase? node)
        {
            if (node is null)
            {
                return 0;
            }

            return _cachedNodeReferences[node].Id;
        }

        public void AddReference(NodeViewModelBase node)
        {
            if (!_cachedNodeReferences.TryGetValue(node, out var cachedReference))
            {
                cachedReference = new NodeReference(this);
                _cachedNodeReferences.Add(node, cachedReference);
            }
            ++cachedReference.ReferenceCount;
        }

        public bool RemoveReference(NodeViewModelBase node)
        {
            var reference = _cachedNodeReferences[node];
			if (--reference.ReferenceCount == 0)
            {
                _cachedNodeReferences.Remove(node);
                return true;
            }
            return false;
        }

        public int Count => _cachedNodeReferences.Count;

        public IEnumerator<KeyValuePair<NodeViewModelBase, ulong>> GetEnumerator()
        {
            foreach (var element in _cachedNodeReferences)
            {
                yield return new(element.Key, element.Value.Id);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
