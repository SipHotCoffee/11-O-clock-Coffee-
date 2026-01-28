using CG.Test.Editor.FrontEnd;
using System.Collections.Immutable;

namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
	public readonly struct LinkedSchemaProperty
    {
        public required string         Name { get; init; }
        public required LinkedSchemaTypeBase Type { get; init; }

        public required int Index { get; init; }
    }

    public class SchemaTypeComparer : IComparer<LinkedSchemaTypeBase>
    {
        public int Compare(LinkedSchemaTypeBase? left, LinkedSchemaTypeBase? right)
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

    public class LinkedSchemaVariantType(string name, IEnumerable<LinkedSchemaTypeBase> possibleTypes) : LinkedSchemaTypeBase, INamedObject
	{
        public IReadOnlySet<LinkedSchemaTypeBase> PossibleTypes { get; } = possibleTypes.ToImmutableSortedSet(new SchemaTypeComparer());

        public string Name { get; } = name;

        public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) 
            => PossibleTypes.Contains(sourceType) || sourceType is LinkedSchemaSymbolType symbolType && PossibleTypes.Contains(symbolType.LinkedType);

        public override string ToString() => Name;
    }

    public class LinkedSchemaObjectType : LinkedSchemaTypeBase, INamedObject
	{
        private readonly Dictionary<string, int> _propertyIndices;

        public LinkedSchemaObjectType(string name, IEnumerable<LinkedSchemaProperty> properties)
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

        public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) 
            => sourceType == this || sourceType is LinkedSchemaSymbolType symbolType && IsConvertibleFrom(symbolType.LinkedType);

        public override string ToString() => Name;
    }
}
