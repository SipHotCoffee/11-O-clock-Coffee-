using System.IO;
using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public abstract class SchemaTypeBase
    {
        public static async Task<SchemaTypeBase?> LoadFromStream(Stream stream, ILogger<SchemaParsingMessage> logger)
        {
			var node = await JsonNode.ParseAsync(stream);

			if (node is JsonObject objectNode && objectNode.TryGetPropertyValue("$defs", out var defsNode) && defsNode is JsonObject defsObjectNode)
			{
				var types = new Dictionary<string, SchemaTypeBase>();
				defsObjectNode.TryParseSchemaDefinitions(logger, types);
				if (objectNode.TryParseLinkedSchemaType(logger, types, out var type))
				{
					return type;
				}
			}

			return null;
		}

        public virtual bool IsValueType => false;

        public abstract bool IsConvertibleFrom(SchemaTypeBase sourceType);

        public abstract override string ToString();
    }
}
