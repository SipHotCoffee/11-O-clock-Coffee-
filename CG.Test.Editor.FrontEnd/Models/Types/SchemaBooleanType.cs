namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaBooleanType(bool defaultValue) : SchemaTypeBase
    {
        public bool DefaultValue { get; } = defaultValue;

		public override bool IsValueType => true;

		public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => sourceType is SchemaBooleanType;

        public override string ToString() => "Boolean";
    }
}
