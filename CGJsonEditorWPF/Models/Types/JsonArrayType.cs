using CG.Test.Editor.Models.Nodes;

namespace CG.Test.Editor.Models.Types
{
    public class JsonArrayType(Type elementType) : JsonTypeBase(elementType.MakeArrayType())
    {
        public JsonTypeBase ElementType { get; } = Get(elementType);

        public override string Name => $"Array<{ElementType.Name}>";

        public override JsonArrayNode Create() => new(GetArrayType(ElementType.Type), []);

        public override bool IsConvertibleFrom(JsonTypeBase type) => type is JsonArrayType arrayType && ElementType.IsConvertibleFrom(arrayType.ElementType);
    }
}
