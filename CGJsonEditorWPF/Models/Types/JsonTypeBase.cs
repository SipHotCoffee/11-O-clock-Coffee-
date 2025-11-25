using CG.Test.Editor.Models.Nodes;

namespace CG.Test.Editor.Models.Types
{
    public struct NullValue;

    public abstract class JsonTypeBase(Type type)
    {
        private static readonly Dictionary<Type, JsonTypeBase> _types = [];

        public abstract string Name { get; }

        public static JsonTypeBase Get<TValue>() => Get(typeof(TValue));

        public static JsonTypeBase Get(Type type)
        {
            if (_types.TryGetValue(type, out var result))
            {
                return result;
            }

            return type.ToJsonType();
        }

        public static JsonArrayType GetArrayType(Type elementType) => (JsonArrayType)Get(elementType.MakeArrayType());

        public Type Type { get; } = type;

        public abstract JsonNodeBase Create();

        public abstract bool IsConvertibleFrom(JsonTypeBase type);
    }
}
