namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaReferenceType(SchemaTypeBase targetType) : SchemaTypeBase
	{
		public SchemaTypeBase TargetType { get; } = targetType;

		public override bool IsConvertibleFrom(SchemaTypeBase sourceType) 
			=> sourceType is SchemaReferenceType sourceReferenceType && TargetType.IsConvertibleFrom(sourceReferenceType.TargetType);

        public override string ToString() => $"{TargetType}@";
    }

	public class SchemaExternalReferenceType(SchemaTypeBase targetType) : SchemaReferenceType(targetType)
	{
		public override bool IsConvertibleFrom(SchemaTypeBase sourceType)
			=> sourceType is SchemaExternalReferenceType sourceReferenceType && TargetType.IsConvertibleFrom(sourceReferenceType.TargetType);

		public override string ToString() => $"{TargetType}$";
	}
}
