using CG.Test.Editor.FrontEnd.Models;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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

        public NodeViewModelBase Visit(SchemaVariantType variantType)
        {
            var visitor = new NodeViewModelGeneratorVisitor(_editor, _parent);

			var possibleTypes = new ObservableCollection<SchemaObjectType>(variantType.PossibleTypes.OfType<SchemaObjectType>());
            if (possibleTypes.Count == 0)
            {
                return new ObjectNodeViewModel(_editor, _parent, new SchemaObjectType(string.Empty, []));
            }
            else if (possibleTypes.Count == 1)
            {
                return possibleTypes[0].Visit(visitor);
            }

			var dialog = new VariantTypeSelectorDialog
            {
                Owner = _editor.OwnerWindow,
                PossibleTypes = possibleTypes
			};

            dialog.ShowDialog();
            return dialog.SelectedType.Visit(visitor);
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

        protected abstract string GetName(NodeViewModelBase item);

		[RelayCommand]
		void Navigate()
		{
			Editor.Navigate(this);
		}
	}
}
