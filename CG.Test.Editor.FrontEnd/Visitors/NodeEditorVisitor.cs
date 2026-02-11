using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using System.Collections.ObjectModel;

namespace CG.Test.Editor.FrontEnd.Visitors
{
    public class NodeEditorVisitor(FileInstanceViewModel editor, bool askForChangingType) : Visitor<NodeEditorVisitor, NodeViewModelBase, Void>
    {
        private readonly FileInstanceViewModel _editor = editor;

        private readonly bool _askForChangingType = askForChangingType;

		public void Visit(ArrayNodeViewModel arrayNode)
        {
            _editor.Navigate(arrayNode);
        }

		public void Visit(ObjectNodeViewModel objectNode)
		{
			_editor.Navigate(objectNode);
		}

        public void Visit(VariantNodeViewModel variantNode)
        {
            if (!_askForChangingType)
            {
                variantNode.SelectedObject.Visit(this);
                return;
            }

            var parameters = new MessageBoxParameters($"Change or edit variant node of type '{variantNode.VariantType.Name}'", "Edit variant node.", "Edit");
            parameters.AddButton("Change Type");
            if (_editor.OwnerWindow.ShowMessage(parameters) == 1)
            {   
                var generatedVariantNode = (VariantNodeViewModel?)variantNode.VariantType.Visit(new NodeViewModelGeneratorVisitor(_editor, variantNode.Parent, null));
                if (generatedVariantNode is null)
                {
                    return;
                }

                variantNode.SelectedObject = generatedVariantNode.SelectedObject;
            }

			variantNode.SelectedObject.Visit(this);
		}

        public void Visit(NumberNodeViewModel numberNode)
        {
            var numberValueDialog = new NumberValueDialog
            {
                Owner      = _editor.OwnerWindow,
                Minimum    = numberNode.Type.Minimum,
                Maximum    = numberNode.Type.Maximum,
                Value      = numberNode.Value,
                IsIntegral = false,
            };

            if (numberValueDialog.ShowDialog() == true)
            {
                numberNode.Value = numberValueDialog.Value;
            }
        }

		public void Visit(IntegerNodeViewModel integerNode)
		{
			var numberValueDialog = new NumberValueDialog
			{
				Owner      = _editor.OwnerWindow,
				Minimum    = integerNode.Type.Minimum,
				Maximum    = integerNode.Type.Maximum,
				Value      = integerNode.Value,
				IsIntegral = true,
			};

			if (numberValueDialog.ShowDialog() == true)
			{
				integerNode.Value = (long)numberValueDialog.Value;
			}
		}

        public void Visit(EnumNodeViewModel enumNode)
        {
            var enumValueDialog = new EnumValueDialog()
            {
                Owner          = _editor.OwnerWindow,
                PossibleValues = new ObservableCollection<string>(enumNode.Type.PossibleValues),
                SelectedIndex  = enumNode.SelectedIndex
            };

            if (enumValueDialog.ShowDialog() == true)
            {
                enumNode.SelectedIndex = enumValueDialog.SelectedIndex;
            }
        }

		public void Visit(BooleanNodeViewModel booleanNode)
        {
            booleanNode.Value = !booleanNode.Value;
        }

		public void Visit(StringNodeViewModel stringNode)
        {
            var stringValueDialog = new StringValueDialog()
            {
				Owner         = _editor.OwnerWindow,
				MaximumLength = stringNode.Type.MaximumLength,
				Text          = stringNode.Value,
			};

			if (stringValueDialog.ShowDialog() == true)
			{
				stringNode.Value = stringValueDialog.Text;
			}
		}

        public void Visit(ReferenceNodeViewModel referenceNode)
        {
            if (referenceNode.Root.AllChildren.Any((node) => referenceNode.Type.TargetType.IsConvertibleFrom(node.Type)))
            {
				var referenceDialog = new ReferencePickerDialog()
				{
					FilterType = referenceNode.Type.TargetType,
					Root = new TreeNodeViewModel(referenceNode.Type.TargetType, referenceNode.Root),
                    SelectedNode = referenceNode.Node,
				};

				if (referenceDialog.ShowDialog() == true)
                {
                    referenceNode.Node = referenceDialog.SelectedNode;
                }
                else
                {
                    referenceNode.Node = null;
                }
            }
            else
            {
				_editor.OwnerWindow.ShowMessage($"No node of type '{referenceNode.Type.TargetType}' can be referenced.");
			}
        }
	}
}
