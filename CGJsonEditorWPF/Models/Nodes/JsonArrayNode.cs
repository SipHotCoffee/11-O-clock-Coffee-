using CG.Test.Editor.Models.Types;
using System.IO;

namespace CG.Test.Editor.Models.Nodes
{
    public class JsonArrayNode(JsonArrayType arrayType, IEnumerable<JsonNodeBase> nodes) : JsonNodeBase
    {
        public List<JsonNodeBase> Elements { get; } = [..nodes];

        public override JsonArrayType Type { get; } = arrayType;

        public override IReadOnlyCollection<JsonNodeBase> Children => Elements;

        public override JsonArrayNode Clone() => new(Type, Elements.Select((node) => node.Clone()));

        public override object Deserialize()
        {
            var result = Array.CreateInstance(Type.ElementType.Type, Elements.Count);
            for (var i = 0; i < Elements.Count; i++)
            {
                result.SetValue(Elements[i].Deserialize(), i);
            }
            return result;
        }

        public override void Serialize(TextWriter target, int tabCount)
        {
            target.WriteLine();
            target.Write($"{new string('\t', tabCount * 4)}[");

            if (Elements.Count != 0)
            {
                for (var i = 0; i < Elements.Count - 1; i++)
                {
                    target.WriteLine();
                    target.Write(new string('\t', (tabCount + 1) * 4));
                    Elements[i].Serialize(target, tabCount + 1);
                    target.Write(',');
                }

                target.WriteLine();
                Elements[^1].Serialize(target, tabCount + 1);
                target.WriteLine();
            }

            target.Write($"{new string('\t', tabCount * 4)}]");
        }

        public override async Task SerializeAsync(TextWriter target, int tabCount)
        {
            await target.WriteLineAsync();
            await target.WriteAsync($"{new string('\t', tabCount * 4)}[");

            if (Elements.Count != 0)
            {
                for (var i = 0; i < Elements.Count - 1; i++)
                {
                    await target.WriteLineAsync();
                    await target.WriteAsync(new string('\t', (tabCount + 1) * 4));
                    await Elements[i].SerializeAsync(target, tabCount + 1);
                    await target.WriteAsync(',');
                }

                await target.WriteLineAsync();
                await Elements[^1].SerializeAsync(target, tabCount + 1);
                await target.WriteLineAsync();
            }

            await target.WriteAsync($"{new string('\t', tabCount * 4)}]");
        }
    }
}
