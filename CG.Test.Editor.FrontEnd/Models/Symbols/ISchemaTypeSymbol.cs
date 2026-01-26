using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.FrontEnd.Models.Symbols
{
    public interface ISchemaTypeSymbol
    {
        bool TryLink(IReadOnlyDictionary<string, LinkedSchemaTypeBase> definedTypes, [NotNullWhen(true)] out LinkedSchemaTypeBase? type);
    }
}
