using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Models
{
    public class SchemaStringType(int maximumLength) : SchemaTypeBase
    {
        public int MaximumLength { get; } = maximumLength;

        public override JsonValue Create() => JsonValue.Create(string.Empty);
    }
}
