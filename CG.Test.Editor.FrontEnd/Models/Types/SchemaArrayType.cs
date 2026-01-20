namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaArrayType(SchemaTypeBase elementType) : SchemaTypeBase
    {
        public SchemaTypeBase ElementType { get; } = elementType;

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) 
            => sourceType is SchemaArrayType sourceArrayType && ElementType.IsConvertibleFrom(sourceArrayType.ElementType);
    }
}
