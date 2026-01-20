using CG.Test.Editor.FrontEnd;
using System.Collections.Immutable;

namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public readonly struct SchemaProperty
    {
        public required string         Name { get; init; }
        public required SchemaTypeBase Type { get; init; }

        public required int Index { get; init; }
    }

    public class SchemaTypeComparer : IComparer<SchemaTypeBase>
    {
        public int Compare(SchemaTypeBase? left, SchemaTypeBase? right)
        {
            if (left is SchemaObjectType leftObjectType && right is SchemaObjectType rightObjectType)
            {
                return leftObjectType.Name.CompareTo(rightObjectType.Name);
            }
            return -1;
        }
    }

    public class SchemaVariantType(IEnumerable<SchemaTypeBase> possibleTypes) : SchemaTypeBase
    {
        public IReadOnlySet<SchemaTypeBase> PossibleTypes { get; } = possibleTypes.ToImmutableSortedSet(new SchemaTypeComparer());

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => PossibleTypes.Contains(sourceType);
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

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => sourceType == this;
    }
}
