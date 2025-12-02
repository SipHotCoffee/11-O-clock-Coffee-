using CG.Test.Editor.FrontEnd.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public class NodeEditorVisitor(FileInstanceViewModel editor) : VisitorVoidBase<NodeViewModelBase>
    {
        private readonly FileInstanceViewModel _editor = editor;

		public void Visit(ArrayNodeViewModel arrayNode)
        {
            _editor.Current = arrayNode;
        }

		public void Visit(ObjectNodeViewModel objectNode)
		{
			_editor.Current = objectNode;
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
	}

    public partial class FileInstanceViewModel : ObservableObject
    {
        private static int _lastId = 1;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
		private NodeViewModelBase _current;

        public FileInstanceViewModel(Window ownerWindow, NodeViewModelBase root)
        {
            OwnerWindow = ownerWindow;

            Name = $"Untitled {_lastId++}";

            Root = root;

            Current = root;
        }

        public Window OwnerWindow { get; }

		public NodeViewModelBase Root { get; }
	}
}
