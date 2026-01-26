namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
    public class LinkedSchemaBooleanType : LinkedSchemaTypeBase
    {
		public override bool IsValueType => true;

		public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) => sourceType is LinkedSchemaBooleanType;
	}
}
