using System.Text.Json.Nodes;

namespace CG.Test.Editor.FrontEnd.Models
{
    public class SchemaIntegerType(long minimum, long maximum) : SchemaTypeBase
	{
		public long Minimum { get; } = minimum;
		public long Maximum { get; } = maximum;

		public override JsonValue Create() => JsonValue.Create(Minimum);
	}
}
