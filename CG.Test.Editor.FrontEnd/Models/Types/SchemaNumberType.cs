namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaNumberType(double minimum, double maximum, double defaultValue) : SchemaTypeBase
    {
        public double Minimum { get; } = minimum;
        public double Maximum { get; } = maximum;

		public double DefaultValue { get; } = defaultValue;

		public override bool IsValueType => true;

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => sourceType is SchemaIntegerType || sourceType is SchemaNumberType;

        public override string ToString() => "Number";
    }
}
