namespace CG.Test.Editor.FrontEnd.Models.Types
{
    public class SchemaSymbolType(string typeName, IReadOnlyDictionary<string, SchemaTypeBase> definedTypes) : SchemaTypeBase, INamedObject
    {
        private readonly IReadOnlyDictionary<string, SchemaTypeBase> _definedTypes = definedTypes;

		public string TypeName { get; } = typeName;

        public SchemaTypeBase LinkedType => _definedTypes[TypeName];

		public override bool IsConvertibleFrom(SchemaTypeBase sourceType) => LinkedType.IsConvertibleFrom(sourceType);

        string INamedObject.Name => TypeName;

        public override string ToString() => TypeName;
    }
}
