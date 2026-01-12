namespace CG.Test.Editor.FrontEnd.Models
{
    public class SchemaReferenceType(SchemaTypeBase elementType) : SchemaTypeBase
	{
		public SchemaTypeBase ElementType { get; } = elementType;

		public override bool IsConvertibleFrom(SchemaTypeBase sourceType) 
			=> sourceType is SchemaReferenceType sourceReferenceType && ElementType.IsConvertibleFrom(sourceReferenceType.ElementType);
	}
}
