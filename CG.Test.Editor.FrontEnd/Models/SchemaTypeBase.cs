using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Models
{
    public abstract class SchemaTypeBase
    {
        public abstract JsonNode Create();
    }
}
