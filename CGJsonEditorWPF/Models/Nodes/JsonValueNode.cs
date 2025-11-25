using CG.Test.Editor.Models.Types;
using System.IO;

namespace CG.Test.Editor.Models.Nodes
{
    public class JsonValueNode(object value) : JsonNodeBase
    {
        public object Value { get; set; } = value;

        public override JsonTypeBase Type => JsonTypeBase.Get(Value.GetType());

        public override IReadOnlyCollection<JsonNodeBase> Children => [];

        public override JsonValueNode Clone() => new(Value);

        public override void Serialize(TextWriter target, int tabCount)
        {
            if (Value is string stringValue)
            {
                target.Write($"\"{stringValue.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t").Replace("\"", "\\\"")}\"");
            }
            else
            {
                target.Write(Value);
            }
        }

        public override async Task SerializeAsync(TextWriter target, int tabCount)
        {
            if (Value is string stringValue)
            {
                await target.WriteAsync($"\"{stringValue.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t").Replace("\"", "\\\"")}\"");
            }
            else
            {
                await target.WriteAsync(Value.ToString());
            }
        }

        public override object Deserialize() => Value;
    }
}
