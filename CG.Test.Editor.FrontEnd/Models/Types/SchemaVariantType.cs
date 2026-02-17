using CG.Test.Editor.FrontEnd;
using System.Collections.Immutable;

namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaVariantType(string name, IEnumerable<SchemaTypeBase> possibleTypes) : SchemaTypeBase, INamedObject
	{
        public IReadOnlySet<SchemaTypeBase> PossibleTypes { get; } = possibleTypes.ToImmutableSortedSet(new SchemaTypeComparer());

        public IReadOnlyDictionary<string, SchemaObjectType> PossibleObjectTypes { get; } = possibleTypes.OfType<SchemaObjectType>().ToImmutableDictionary((type) => type.Name, (type) => type);

        public string Name { get; } = name;

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) 
            => PossibleTypes.Contains(sourceType) || sourceType is SchemaSymbolType symbolType && PossibleTypes.Contains(symbolType.LinkedType);

        public override string ToString() => Name;
    }
}
