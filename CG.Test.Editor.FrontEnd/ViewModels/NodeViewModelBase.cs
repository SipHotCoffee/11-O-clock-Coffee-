using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public class NodeViewModelGeneratorVisitor(NodeViewModelBase? parent) : VisitorBase<SchemaTypeBase, NodeViewModelBase>
    {
        private readonly NodeViewModelBase? _parent = parent;

        public ArrayNodeViewModel Visit(SchemaArrayType arrayType) => new(_parent, arrayType);

        public ObjectNodeViewModel Visit(SchemaObjectType objectType)
        {
			var result = new ObjectNodeViewModel(_parent, objectType);

			foreach (var property in objectType.Properties)
			{
				result.Nodes.Add(new KeyValuePair<string, NodeViewModelBase>(property.Name, property.Type.Visit(new NodeViewModelGeneratorVisitor(result))));
			}

            return result;
        }

        public NumberNodeViewModel Visit(SchemaNumberType numberType) => new(_parent, numberType, Math.Min(Math.Max(numberType.Minimum, 0), numberType.Maximum));

        public IntegerNodeViewModel Visit(SchemaIntegerType integerType) => new(_parent, integerType, Math.Min(Math.Max(integerType.Minimum, 0), integerType.Maximum));

        public StringNodeViewModel Visit(SchemaStringType stringType) => new(_parent, stringType, string.Empty);

		public BooleanNodeViewModel Visit(SchemaBooleanType booleanType) => new(_parent, booleanType, false);
	}

    public abstract partial class NodeViewModelBase(NodeViewModelBase? parent) : ObservableObject
    {
        public NodeViewModelBase? Parent { get; } = parent;

        public string Name => Parent?.GetName(this) ?? "(Root)";

        public abstract SchemaTypeBase Type { get; }

        public abstract NodeViewModelBase Clone(NodeViewModelBase? parent);

        protected virtual string GetName(NodeViewModelBase item) => "CHILD NOT FOUND!!!";
	}
}
