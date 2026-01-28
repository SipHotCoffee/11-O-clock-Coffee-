namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
    public class LinkedSchemaArrayType(LinkedSchemaTypeBase elementType) : LinkedSchemaTypeBase
    {
        public LinkedSchemaTypeBase ElementType { get; } = elementType;

        public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) 
            => sourceType is LinkedSchemaArrayType sourceArrayType && ElementType.IsConvertibleFrom(sourceArrayType.ElementType);

        public override string ToString() => $"{ElementType}[]";
    }
}
