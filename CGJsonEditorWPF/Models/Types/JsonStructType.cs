using CG.Test.Editor.Models.Nodes;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.Models.Types
{
    public class JsonField
    {
        public required string Name { get; init; }

        public required JsonTypeBase Type { get; init; }

        public required int Index { get; init; }
    }

    public class JsonStructType : JsonTypeBase
    {
        private readonly Dictionary<string, int> _fieldIndices;

        private readonly JsonStructType[] _derivedTypes;

        public JsonStructType(Type type, IEnumerable<JsonField> fields) : base(type)
        {
            _fieldIndices = [];

            Name = type.Name;

            Fields = [.. fields.Select((field, index) =>
            {
                _fieldIndices.Add(field.Name, index);
                return field;
            })];

            _derivedTypes = [.. type.EnumerateDerivedTypes().Select((derivedType) => (JsonStructType)Get(derivedType))];
        }

        public override string Name { get; }

        public ImmutableArray<JsonField> Fields { get; }

        public IReadOnlyCollection<JsonStructType> DerivedTypes => _derivedTypes;

        public bool TryGetField(string name, [NotNullWhen(true)] out JsonField? field)
        {
            if (_fieldIndices.TryGetValue(name, out var fieldIndex))
            {
                field = Fields[fieldIndex];
                return true;
            }
            field = null;
            return false;
        }

        public override JsonObjectNode Create()
        {
            var propertyValues = new JsonNodeBase[Fields.Length];
            for (var i = 0; i < Fields.Length; i++)
            {
                propertyValues[i] = Fields[i].Type.Create();
            }
            return new JsonObjectNode(this, propertyValues);
        }

        public override bool IsConvertibleFrom(JsonTypeBase type) => Type.IsAssignableFrom(type.Type);
    }
}
