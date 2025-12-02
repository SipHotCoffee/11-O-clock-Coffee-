using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Models
{
    public readonly struct SchemaProperty
    {
        public required string         Name { get; init; }
        public required SchemaTypeBase Type { get; init; }

        public required int Index { get; init; }
    }

    public class SchemaObjectType(string name, IReadOnlyDictionary<string, SchemaProperty> properties) : SchemaTypeBase
    {
        public string Name { get; } = name;

        public IReadOnlyDictionary<string, SchemaProperty> Properties { get; } = properties;

        public override JsonObject Create()
        {
            var result = new JsonObject();

            foreach (var schemaProperty in Properties.Values)
            {
                result.Add(schemaProperty.Name, schemaProperty.Type.Create());
            }

            return result;
        }
    }
}
