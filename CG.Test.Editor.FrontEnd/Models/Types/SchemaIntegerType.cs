namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaIntegerType(long minimum, long maximum, long defaultValue) : SchemaTypeBase
	{
		public long Minimum { get; } = minimum;
		public long Maximum { get; } = maximum;

		public long DefaultValue { get; } = defaultValue;

		public override bool IsValueType => true;

		public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => sourceType is SchemaIntegerType;

		public override string ToString() => "Integer";
	}
}
