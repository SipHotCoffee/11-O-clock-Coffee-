using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd
{
    public readonly struct SchemaParsingMessage(string message, JsonNode node)
    {
        public string Message { get; } = message;

        public JsonNode Node { get; } = node;
    }
}
