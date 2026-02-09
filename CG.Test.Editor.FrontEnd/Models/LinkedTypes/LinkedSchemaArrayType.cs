namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
    public class LinkedSchemaArrayType(LinkedSchemaTypeBase elementType, int minimumItemCount, int maximumItemCount) : LinkedSchemaTypeBase
    {
        public LinkedSchemaTypeBase ElementType { get; } = elementType;

        public int MinimumItemCount { get; } = minimumItemCount;
        public int MaximumItemCount { get; } = maximumItemCount;

        public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) 
            => sourceType is LinkedSchemaArrayType sourceArrayType && ElementType.IsConvertibleFrom(sourceArrayType.ElementType);

        public override string ToString() => $"{ElementType}[]";
    }
}
