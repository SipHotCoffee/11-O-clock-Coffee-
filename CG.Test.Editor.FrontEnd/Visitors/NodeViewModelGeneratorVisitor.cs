using CG.Test.Editor.FrontEnd.Models.Types;
using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace CG.Test.Editor.FrontEnd.Visitors
{
    public class NodeViewModelGeneratorVisitor(Window ownerWindow, NodeTree tree, NodeViewModelBase? parent, string? propertyName) : Visitor<NodeViewModelGeneratorVisitor, SchemaTypeBase, NodeViewModelBase?>
    {
        private readonly Window _ownerWindow = ownerWindow;

        private readonly NodeTree _tree = tree;

        private readonly NodeViewModelBase? _parent = parent;

        private readonly string? _propertyName = propertyName;

		public NodeViewModelBase? Visit(SchemaSymbolType symbolType) => Invoke(symbolType.LinkedType);

        public ArrayNodeViewModel? Visit(SchemaArrayType arrayType)
        {
            var nodes = Enumerable.Range(0, arrayType.MinimumItemCount).Select((index) => arrayType.ElementType.Visit(this));
            if (nodes.Any((node) => node is null))
            {
                return null;
            }

			return new ArrayNodeViewModel(_tree, _parent, nodes.OfType<NodeViewModelBase>(), arrayType);
        }

        public ObjectNodeViewModel? Visit(SchemaObjectType objectType)
        {
			var result = new ObjectNodeViewModel(_tree, _parent, objectType);

			foreach (var property in objectType.Properties)
			{
                var node = property.Type.Visit(new NodeViewModelGeneratorVisitor(_ownerWindow, _tree, result, property.Name));
                if (node is null)
                {
                    return null;
                }

                result.Nodes.Add(new KeyValuePair<string, NodeViewModelBase>(property.Name, node));
			}
            
            return result;
        }

        public NodeViewModelBase? Visit(SchemaVariantType variantType)
        {
			var possibleTypes = new ObservableCollection<SchemaObjectType>(variantType.EnumerateObjectTypes());
            if (possibleTypes.Count == 0)
            {
                return null;
            }

            if (possibleTypes.Count == 1)
            {
                return possibleTypes[0].Visit(this);
            }

            var dialog = new VariantTypeSelectorDialog
            {
                Owner = _ownerWindow,
                PossibleTypes = CollectionViewSource.GetDefaultView(possibleTypes)
            };

			if (_propertyName is not null)
			{
                dialog.Title = $"Select variant type for property: '{_propertyName}'";
			}

			if (dialog.ShowDialog() == true && dialog.SelectedType.Visit(this) is ObjectNodeViewModel objectNode)
            {
				return new VariantNodeViewModel(_tree, _parent, variantType, objectNode);
			}

            return null;
        }

        public EnumNodeViewModel? Visit(SchemaEnumType enumType) => new(_tree, _parent, enumType, 0);

		public NumberNodeViewModel? Visit(SchemaNumberType numberType) => new(_tree, _parent, numberType, Math.Min(Math.Max(numberType.Minimum, 0), numberType.Maximum));

        public IntegerNodeViewModel? Visit(SchemaIntegerType integerType) => new(_tree, _parent, integerType, Math.Min(Math.Max(integerType.Minimum, 0), integerType.Maximum));

        public StringNodeViewModel? Visit(SchemaStringType stringType) => new(_tree, _parent, stringType, string.Empty);

		public BooleanNodeViewModel? Visit(SchemaBooleanType booleanType) => new(_tree, _parent, booleanType, false);

        public ReferenceNodeViewModel? Visit(SchemaReferenceType referenceType) => new(_tree, _parent, referenceType, null);
	}
}
