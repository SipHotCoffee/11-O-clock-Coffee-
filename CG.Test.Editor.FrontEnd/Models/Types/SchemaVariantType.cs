using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaVariantType : SchemaTypeBase, INamedObject
	{
        private readonly Dictionary<string, SchemaTypeBase> _possibleObjectTypes;

		public SchemaVariantType(string name, IEnumerable<SchemaTypeBase> possibleTypes)
		{
			_possibleObjectTypes = [];
			foreach (var possibleType in possibleTypes)
			{
				if (possibleType is INamedObject namedObject)
				{
					_possibleObjectTypes.Add(namedObject.Name, possibleType);
				}
			}

			PossibleTypes = possibleTypes.ToImmutableSortedSet(new SchemaTypeComparer());

			Name = name;
		}

		public bool TryGetObjectType(string name, [NotNullWhen(true)] out SchemaObjectType? objectType)
		{
			if (_possibleObjectTypes.TryGetValue(name, out var possibleType))
			{
				if (possibleType is SchemaObjectType possibleObjectType)
				{
					objectType = possibleObjectType;
					return true;
				}
				else if (possibleType is SchemaSymbolType possibleSymbolType && possibleSymbolType.LinkedType is SchemaObjectType linkedObjectType)
				{
					objectType = linkedObjectType;
					return true;
				}
			}
			objectType = null;
			return false;
		}

		public IReadOnlySet<SchemaTypeBase> PossibleTypes { get; }

        public string Name { get; }

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) 
            => PossibleTypes.Contains(sourceType) || sourceType is SchemaSymbolType symbolType && PossibleTypes.Contains(symbolType.LinkedType);

        public override string ToString() => Name;
    }
}
