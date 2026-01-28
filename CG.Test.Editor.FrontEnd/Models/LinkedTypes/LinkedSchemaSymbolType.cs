namespace CG.Test.Editor.FrontEnd.Models.LinkedTypes
{
    public class LinkedSchemaSymbolType(string typeName, IReadOnlyDictionary<string, LinkedSchemaTypeBase> definedTypes) : LinkedSchemaTypeBase, INamedObject
    {
        private readonly IReadOnlyDictionary<string, LinkedSchemaTypeBase> _definedTypes = definedTypes;

		public string TypeName { get; } = typeName;

        public LinkedSchemaTypeBase LinkedType => _definedTypes[TypeName];

		public override bool IsConvertibleFrom(LinkedSchemaTypeBase sourceType) => LinkedType.IsConvertibleFrom(sourceType);

        string INamedObject.Name => TypeName;

        public override string ToString() => LinkedType.ToString();
    }
}
