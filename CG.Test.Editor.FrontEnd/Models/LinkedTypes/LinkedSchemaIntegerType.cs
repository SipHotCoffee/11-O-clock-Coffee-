namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
    public class LinkedSchemaIntegerType(long minimum, long maximum) : LinkedSchemaTypeBase
	{
		public long Minimum { get; } = minimum;
		public long Maximum { get; } = maximum;

		public override bool IsValueType => true;

		public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) => sourceType is LinkedSchemaIntegerType;

		public override string ToString() => "Integer";
	}
}
