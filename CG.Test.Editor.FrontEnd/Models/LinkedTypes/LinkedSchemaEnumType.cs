namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
    public class LinkedSchemaEnumType(IEnumerable<string> possibleValues) : LinkedSchemaTypeBase
    {
        private readonly Dictionary<string, int> _possibleValueSet = possibleValues.Select((stringValue, index) => new KeyValuePair<string, int>(stringValue, index)).ToDictionary();

        public IReadOnlyList<string> PossibleValues { get; } = [.. possibleValues];

        public bool TryFindIndex(string name, out int index) => _possibleValueSet.TryGetValue(name, out index);

		public override bool IsValueType => true;

		public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) => sourceType == this;
	}
}
