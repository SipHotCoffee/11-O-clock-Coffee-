using CG.Test.Editor.Models.Types;
using System.IO;

namespace CG.Test.Editor.Models.Nodes
{
    public abstract class JsonNodeBase
    {
        public abstract IReadOnlyCollection<JsonNodeBase> Children { get; }

        public abstract JsonTypeBase Type { get; }

        public abstract JsonNodeBase Clone();

        public abstract void Serialize(TextWriter target, int tabCount);

        public abstract Task SerializeAsync(TextWriter target, int tabCount);

        public abstract object Deserialize();
    }
}
