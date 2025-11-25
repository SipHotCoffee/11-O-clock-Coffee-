using CG.Test.Editor.Models.Types;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.Models.Nodes
{
    public class JsonObjectNode(JsonStructType type, IEnumerable<JsonNodeBase> nodes) : JsonObjectNodeBase
    {
        private readonly JsonStructType _type = type;

        public ImmutableArray<JsonNodeBase> Values { get; } = [.. nodes];

        public override IReadOnlyCollection<JsonNodeBase> Children => Values;

        public override JsonStructType Type { get; } = type;

        public override bool TryGetValue(string name, [NotNullWhen(true)] out JsonNodeBase? value)
        {
            if (_type.TryGetField(name, out var field))
            {
                value = Values[field.Index];
                return true;
            }
            value = null;
            return false;
        }

        public override JsonObjectNode Clone() => new(_type, Values.Select((node) => node.Clone()));

        public override IEnumerable<KeyValuePair<string, JsonNodeBase>> EnumerateNodes() 
            => Values.Select((node, index) => new KeyValuePair<string, JsonNodeBase>(_type.Fields[index].Name, node))
                     .Prepend(new KeyValuePair<string, JsonNodeBase>("type", new JsonValueNode(_type.Name)));

        public override object Deserialize() => Activator.CreateInstance(Type.Type, Values.Select((node) => node.Deserialize()))!;
    }
}
