using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Xml.Linq;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public class NodeViewModelGeneratorVisitor(FileInstanceViewModel editor, NodeViewModelBase? parent) : VisitorBase<SchemaTypeBase, NodeViewModelBase>
    {
        private readonly FileInstanceViewModel _editor = editor;

        private readonly NodeViewModelBase? _parent = parent;

        public ArrayNodeViewModel Visit(SchemaArrayType arrayType) => new(_editor, _parent, arrayType);

        public ObjectNodeViewModel Visit(SchemaObjectType objectType)
        {
			var result = new ObjectNodeViewModel(_editor, _parent, objectType);

			foreach (var property in objectType.Properties)
			{
				result.Nodes.Add(new KeyValuePair<string, NodeViewModelBase>(property.Name, property.Type.Visit(new NodeViewModelGeneratorVisitor(_editor, result))));
			}

            return result;
        }

        public NumberNodeViewModel Visit(SchemaNumberType numberType) => new(_editor, _parent, numberType, Math.Min(Math.Max(numberType.Minimum, 0), numberType.Maximum));

        public IntegerNodeViewModel Visit(SchemaIntegerType integerType) => new(_editor, _parent, integerType, Math.Min(Math.Max(integerType.Minimum, 0), integerType.Maximum));

        public StringNodeViewModel Visit(SchemaStringType stringType) => new(_editor, _parent, stringType, string.Empty);

		public BooleanNodeViewModel Visit(SchemaBooleanType booleanType) => new(_editor, _parent, booleanType, false);
	}

    public abstract partial class NodeViewModelBase(FileInstanceViewModel editor, NodeViewModelBase? parent) : ObservableObject
    {
        public FileInstanceViewModel Editor { get; } = editor;

        public NodeViewModelBase? Parent { get; } = parent;

        public string Name => Parent?.GetName(this) ?? "(Root)";

        public abstract SchemaTypeBase Type { get; }

        public abstract NodeViewModelBase Clone(NodeViewModelBase? parent);

        protected virtual string GetName(NodeViewModelBase item) => "CHILD NOT FOUND!!!";

		[RelayCommand]
		void Navigate()
		{
			Editor.Navigate(this);
		}
	}
}
