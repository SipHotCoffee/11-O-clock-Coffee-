namespace CG.Test.Editor.FrontEnd.Models
{
    public class SchemaBooleanType : SchemaTypeBase
    {
		public override bool IsValueType => true;

		public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => sourceType is SchemaBooleanType;
    }
}
