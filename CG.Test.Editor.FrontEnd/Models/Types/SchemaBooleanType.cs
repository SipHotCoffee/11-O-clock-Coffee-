namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaBooleanType : SchemaTypeBase
    {
		public override bool IsValueType => true;

		public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => sourceType is SchemaBooleanType;

        public override string ToString() => "Boolean";
    }
}
