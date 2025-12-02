using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Models
{
    public readonly struct SchemaProperty
    {
        public required string         Name { get; init; }
        public required SchemaTypeBase Type { get; init; }

        public required int Index { get; init; }
    }

    public class SchemaObjectType : SchemaTypeBase
    {
        private readonly Dictionary<string, int> _propertyIndices;

        public SchemaObjectType(string name, IEnumerable<SchemaProperty> properties)
        {
            Name       = name;
            Properties = [.. properties];

			_propertyIndices = [];
            for (var i = 0; i < Properties.Count; i++)
            {
                _propertyIndices.Add(Properties[i].Name, i);
            }
		}

		public string Name { get; }

		public IReadOnlyList<SchemaProperty> Properties { get; }

        public bool TryGetProperty(string propertyName, out SchemaProperty property)
        {
            if (_propertyIndices.TryGetValue(propertyName, out var index))
            {
                property = Properties[index];
                return true;
            }
            property = default;
            return false;
        }

		public override JsonObject Create()
        {
            var result = new JsonObject();

            foreach (var schemaProperty in Properties)
            {
                result.Add(schemaProperty.Name, schemaProperty.Type.Create());
            }

            return result;
        }
    }
}
