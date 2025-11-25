using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CG.Test.Editor.Models.Nodes
{
    public abstract class JsonObjectNodeBase : JsonNodeBase
    {
        public abstract bool TryGetValue(string name, [NotNullWhen(true)] out JsonNodeBase? value);

        public abstract IEnumerable<KeyValuePair<string, JsonNodeBase>> EnumerateNodes();

        public override void Serialize(TextWriter target, int tabCount)
        {
            target.WriteLine();
            target.Write($"{new string('\t', tabCount * 4)}{{");

            using var enumerator = EnumerateNodes().GetEnumerator();
            if (enumerator.MoveNext())
            {
                if (!enumerator.MoveNext())
                {
                    target.WriteLine();
                    var (firstName, firstNode) = enumerator.Current;
                    target.Write(new string('\t', (tabCount + 1) * 4));
                    target.Write($"{firstName}: ");
                    firstNode.Serialize(target, tabCount + 1);
                    target.WriteLine();
                }
                else
                {
                    do
                    {
                        target.WriteLine();
                        var (name, node) = enumerator.Current;
                        target.Write(new string('\t', (tabCount + 1) * 4));
                        target.Write($"{name}: ");
                        node.Serialize(target, tabCount + 1);
                        target.Write(',');
                    }
                    while (enumerator.MoveNext());
                }
            }
            target.Write($"{new string('\t', tabCount * 4)}}}");
        }

        public override async Task SerializeAsync(TextWriter target, int tabCount)
        {
            await target.WriteLineAsync();
            await target.WriteAsync($"{new string('\t', tabCount * 4)}{{");

            using var enumerator = EnumerateNodes().GetEnumerator();
            if (enumerator.MoveNext())
            {
                if (!enumerator.MoveNext())
                {
                    await target.WriteLineAsync();
                    var (firstName, firstNode) = enumerator.Current;
                    await target.WriteAsync(new string('\t', (tabCount + 1) * 4));
                    await target.WriteAsync($"{firstName}: ");
                    await firstNode.SerializeAsync(target, tabCount + 1);
                    await target.WriteLineAsync();
                }
                else
                {
                    do
                    {
                        await target.WriteLineAsync();
                        var (name, node) = enumerator.Current;
                        await target.WriteAsync(new string('\t', (tabCount + 1) * 4));
                        await target.WriteAsync($"{name}: ");
                        await node.SerializeAsync(target, tabCount + 1);
                        await target.WriteAsync(',');
                    }
                    while (enumerator.MoveNext());
                }
            }
            await target.WriteAsync($"{new string('\t', tabCount * 4)}}}");
        }
    }
}
