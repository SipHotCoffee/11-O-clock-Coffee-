namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
    public class LinkedSchemaReferenceType(LinkedSchemaTypeBase elementType) : LinkedSchemaTypeBase
	{
		public LinkedSchemaTypeBase ElementType { get; } = elementType;

		public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) 
			=> sourceType is LinkedSchemaReferenceType sourceReferenceType && ElementType.IsConvertibleFrom(sourceReferenceType.ElementType);
	}
}
