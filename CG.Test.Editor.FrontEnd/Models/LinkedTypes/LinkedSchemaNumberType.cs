namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
    public class LinkedSchemaNumberType(double minimum, double maximum) : LinkedSchemaTypeBase
    {
		public double Minimum { get; } = minimum;
        public double Maximum { get; } = maximum;

		public override bool IsValueType => true;

		public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) => sourceType is LinkedSchemaIntegerType || sourceType is LinkedSchemaNumberType;
	}
}
