namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaStringType(int maximumLength, string defaultValue) : SchemaTypeBase
    {
        public int MaximumLength { get; } = maximumLength;

        public string DefaultValue { get; } = defaultValue;

        public override bool IsValueType => true;

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => sourceType is SchemaStringType;

        public override string ToString() => "String";
    }
}
