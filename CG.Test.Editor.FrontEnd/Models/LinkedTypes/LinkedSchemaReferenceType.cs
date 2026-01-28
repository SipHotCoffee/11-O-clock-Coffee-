namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
    public class LinkedSchemaReferenceType(LinkedSchemaTypeBase targetType) : LinkedSchemaTypeBase
	{
		public LinkedSchemaTypeBase TargetType { get; } = targetType;

		public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) 
			=> sourceType is LinkedSchemaReferenceType sourceReferenceType && TargetType.IsConvertibleFrom(sourceReferenceType.TargetType);

        public override string ToString() => $"{TargetType}&";
    }
}
