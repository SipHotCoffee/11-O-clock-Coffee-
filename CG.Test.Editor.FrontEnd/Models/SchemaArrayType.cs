using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Models
{
    public class SchemaArrayType(SchemaTypeBase elementType) : SchemaTypeBase
    {
        public SchemaTypeBase ElementType { get; } = elementType;

        public override JsonArray Create() => [];
    }
}
