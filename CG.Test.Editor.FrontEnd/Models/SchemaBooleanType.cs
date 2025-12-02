using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Models
{
    public class SchemaBooleanType : SchemaTypeBase
    {
        public override JsonNode Create() => JsonValue.Create(false);
    }
}
