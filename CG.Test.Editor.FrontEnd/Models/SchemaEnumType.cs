namespace CG.Test.Editor.FrontEnd.Models
{
    public class SchemaEnumType(IEnumerable<string> possibleValues) : SchemaTypeBase
    {
        public IReadOnlyList<string> PossibleValues { get; } = [.. possibleValues];

        public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => sourceType == this;
    }
}
