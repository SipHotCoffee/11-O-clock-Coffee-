using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.FrontEnd.Models.Symbols
{
    public class SchemaNamedTypeSymbol(string name) : ISchemaTypeSymbol
    {
        public string Name { get; } = name;

        public bool TryLink(IReadOnlyDictionary<string, LinkedSchemaTypeBase> definedTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type) 
            => definedTypes.TryGetValue(Name, out type);
    }
}
