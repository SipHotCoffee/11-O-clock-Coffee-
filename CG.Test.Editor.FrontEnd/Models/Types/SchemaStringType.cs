namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaStringType(int maximumLength) : SchemaTypeBase
    {
        public int MaximumLength { get; } = maximumLength;

        public override bool IsValueType => true;

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => sourceType is SchemaStringType;
    }
}
