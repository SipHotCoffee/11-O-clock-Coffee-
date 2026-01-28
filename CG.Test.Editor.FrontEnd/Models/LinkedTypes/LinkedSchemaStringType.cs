namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
    public class LinkedSchemaStringType(int maximumLength) : LinkedSchemaTypeBase
    {
        public int MaximumLength { get; } = maximumLength;

        public override bool IsValueType => true;

        public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) => sourceType is LinkedSchemaStringType;

        public override string ToString() => "String";
    }
}
