using CG.Test.Editor.FrontEnd.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
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
	}

    public partial class AddressItem(FileInstanceViewModel editor, NodeViewModelBase node) : ObservableObject
    {
        private readonly FileInstanceViewModel _editor = editor;

        public NodeViewModelBase Node { get; } = node;

		[RelayCommand]
        void Navigate()
        {
            _editor.Navigate(Node);
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

			AddressItems = [];

			Root = root;

            Current = root;
		}

        partial void OnCurrentChanged(NodeViewModelBase? oldValue, NodeViewModelBase newValue)
        {
            AddressItems.Clear();
            for (var current = Current; current is not null; current = current.Parent)
            {
                AddressItems.Insert(0, new AddressItem(this, current));
            }
		}

        public Window OwnerWindow { get; }

		public NodeViewModelBase Root { get; }

        public ObservableCollection<AddressItem> AddressItems { get; }

        [RelayCommand]
        void MoveUp()
        {
            Current = Current.Parent ?? throw new NullReferenceException();
        }

        public void Navigate(NodeViewModelBase target)
        {
            Current = target;
        }
	}
}
