using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaIntegerType(long minimum, long maximum) : SchemaTypeBase
	{
		public long Minimum { get; } = minimum;
		public long Maximum { get; } = maximum;

		public override bool IsValueType => true;

		public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => sourceType is SchemaIntegerType;
    }
}
