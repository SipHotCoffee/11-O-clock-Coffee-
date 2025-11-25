using CG.Test.Editor.Models.Types;
using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.Models.Nodes
{
    public class JsonMapNode : JsonObjectNodeBase
    {
        public Dictionary<string, JsonNodeBase> Nodes { get; } = [];

        public override IReadOnlyCollection<JsonNodeBase> Children => Nodes.Values;

        public override JsonTypeBase Type => throw new NotImplementedException();

        public override bool TryGetValue(string name, [NotNullWhen(true)] out JsonNodeBase? value) => Nodes.TryGetValue(name, out value);

        public override JsonMapNode Clone()
        {
            var result = new JsonMapNode();
            foreach (var (name, node) in Nodes)
            {
                result.Nodes.Add(name, node.Clone());
            }
            return result;
        }

        public override IEnumerable<KeyValuePair<string, JsonNodeBase>> EnumerateNodes() => Nodes;

        public override object Deserialize() => Nodes.Select((pair) => new KeyValuePair<string, object>(pair.Key, pair.Value.Deserialize())).ToDictionary();
    }
}
