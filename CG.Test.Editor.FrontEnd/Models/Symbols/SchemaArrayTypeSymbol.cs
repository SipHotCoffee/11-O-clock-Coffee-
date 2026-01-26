using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.FrontEnd.Models.Symbols
{
    public class SchemaArrayTypeSymbol(LinkedSchemaTypeBase elementType) : ISchemaTypeSymbol
    {
        public LinkedSchemaTypeBase ElementType { get; } = elementType;

        public bool TryLink(IReadOnlyDictionary<string, LinkedSchemaTypeBase> definedTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type)
        {
            type = new LinkedSchemaArrayType(ElementType);
            return true;
        }
    }
}
