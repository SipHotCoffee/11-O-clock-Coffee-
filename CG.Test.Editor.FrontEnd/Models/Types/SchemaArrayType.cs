namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaArrayType(SchemaTypeBase elementType, int minimumItemCount, int maximumItemCount) : SchemaTypeBase
    {
        public SchemaTypeBase ElementType { get; } = elementType;

        public int MinimumItemCount { get; } = minimumItemCount;
        public int MaximumItemCount { get; } = maximumItemCount;

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) 
            => sourceType is SchemaArrayType sourceArrayType && ElementType.IsConvertibleFrom(sourceArrayType.ElementType);

        public override string ToString() => $"{ElementType}[]";
    }
}
