namespace CG.Test.Editor.FrontEnd.Models.Types
{
	public readonly struct LinkedSchemaProperty
    {
        public required string         Name { get; init; }
        public required SchemaTypeBase Type { get; init; }

        public required int Index { get; init; }
    }

    public class SchemaTypeComparer : IComparer<SchemaTypeBase>
    {
        public int Compare(SchemaTypeBase? left, SchemaTypeBase? right)
        {
			if (left is INamedObject leftType && right is INamedObject rightType)
            {
                return leftType.Name.CompareTo(rightType.Name);
            }

			return -1;
        }
    }

    public interface INamedObject
    {
        string Name { get; }
    }

    public class SchemaObjectType : SchemaTypeBase, INamedObject
	{
        private readonly Dictionary<string, int> _propertyIndices;

        public SchemaObjectType(string name, IEnumerable<LinkedSchemaProperty> properties)
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

		public IReadOnlyList<LinkedSchemaProperty> Properties { get; }

        public bool TryGetProperty(string propertyName, out LinkedSchemaProperty property)
        {
            if (_propertyIndices.TryGetValue(propertyName, out var index))
            {
                property = Properties[index];
                return true;
            }
            property = default;
            return false;
        }

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) 
            => sourceType == this || sourceType is SchemaSymbolType symbolType && IsConvertibleFrom(symbolType.LinkedType);

        public override string ToString() => Name;
    }
}
