using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using System.Collections.ObjectModel;

namespace CG.Test.Editor.FrontEnd.Visitors
{
    public class NodeEditorVisitor(FileInstanceViewModel editor) : Visitor<NodeEditorVisitor, NodeViewModelBase, Void>
    {
        private readonly FileInstanceViewModel _editor = editor;

		public void Visit(ArrayNodeViewModel arrayNode)
        {
            _editor.Navigate(arrayNode);
        }

		public void Visit(ObjectNodeViewModel objectNode)
		{
			_editor.Navigate(objectNode);
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
            var referenceDialog = new ReferencePickerDialog()
            {
                Root = new TreeNodeViewModel(referenceNode.Type.ElementType, referenceNode.Root)
            };
            referenceDialog.ShowDialog();
        }
	}
}
