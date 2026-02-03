using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace CG.Test.Editor.FrontEnd.Visitors
{
    public class NodeViewModelGeneratorVisitor(FileInstanceViewModel editor, NodeViewModelBase? parent, string? propertyName) : Visitor<NodeViewModelGeneratorVisitor, LinkedSchemaTypeBase, NodeViewModelBase>
    {
        private readonly FileInstanceViewModel _editor = editor;

        private readonly NodeViewModelBase? _parent = parent;

        private readonly string? _propertyName = propertyName;

		public NodeViewModelBase Visit(LinkedSchemaSymbolType symbolType) => Invoke(symbolType.LinkedType);

		public ArrayNodeViewModel Visit(LinkedSchemaArrayType arrayType) => new(_editor, _parent, arrayType);

        public ObjectNodeViewModel Visit(LinkedSchemaObjectType objectType)
        {
			var result = new ObjectNodeViewModel(_editor, _parent, objectType);

			foreach (var property in objectType.Properties)
			{
				result.Nodes.Add(new KeyValuePair<string, NodeViewModelBase>(property.Name, property.Type.Visit(new NodeViewModelGeneratorVisitor(_editor, result, property.Name))));
			}
            
            return result;
        }

        public NodeViewModelBase Visit(LinkedSchemaVariantType variantType)
        {
			var possibleTypes = new ObservableCollection<LinkedSchemaObjectType>(variantType.EnumerateObjectTypes());
            if (possibleTypes.Count == 0)
            {
                return new ObjectNodeViewModel(_editor, _parent, new LinkedSchemaObjectType(string.Empty, []));
            }
            else if (possibleTypes.Count == 1)
            {
                return possibleTypes[0].Visit(this);
            }

            var dialog = new VariantTypeSelectorDialog
            {
                Owner = _editor.OwnerWindow,
                PossibleTypes = CollectionViewSource.GetDefaultView(possibleTypes)
            };

			if (_propertyName is not null)
			{
                dialog.Title = $"Select variant type for property: '{_propertyName}'";
			}

			dialog.ShowDialog();
            return dialog.SelectedType.Visit(this);
        }

        public EnumNodeViewModel Visit(LinkedSchemaEnumType enumType) => new(_editor, _parent, enumType, 0);

		public NumberNodeViewModel Visit(LinkedSchemaNumberType numberType) => new(_editor, _parent, numberType, Math.Min(Math.Max(numberType.Minimum, 0), numberType.Maximum));

        public IntegerNodeViewModel Visit(LinkedSchemaIntegerType integerType) => new(_editor, _parent, integerType, Math.Min(Math.Max(integerType.Minimum, 0), integerType.Maximum));

        public StringNodeViewModel Visit(LinkedSchemaStringType stringType) => new(_editor, _parent, stringType, string.Empty);

		public BooleanNodeViewModel Visit(LinkedSchemaBooleanType booleanType) => new(_editor, _parent, booleanType, false);

        public ReferenceNodeViewModel Visit(LinkedSchemaReferenceType referenceType) => new(_editor, _parent, referenceType, null);
	}
}
