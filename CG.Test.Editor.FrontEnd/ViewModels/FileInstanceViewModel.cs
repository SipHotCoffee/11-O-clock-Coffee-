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

        private readonly List<NodeViewModelBase> _history;

        [ObservableProperty]
        private int _historyIndex;

        [ObservableProperty]
        private bool _isBackButtonEnabled;

        [ObservableProperty]
        private bool _isForwardButtonEnabled;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
		private NodeViewModelBase _current;

        public FileInstanceViewModel(Window ownerWindow, NodeViewModelBase root)
        {
            _history = [];

            _historyIndex = 0;

			OwnerWindow = ownerWindow;

            Name = $"Untitled {_lastId++}";

			AddressItems = [];

			Root = root;

            Current = root;
            AddPage();
		}

        partial void OnHistoryIndexChanged(int oldValue, int newValue)
        {
            IsBackButtonEnabled = newValue > 0;
            IsForwardButtonEnabled = newValue < _history.Count - 1;
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

        private void AddPage()
        {
            var nextIndex = HistoryIndex + 1;
            if (nextIndex < _history.Count)
            {
                _history.RemoveRange(nextIndex, _history.Count - nextIndex);
            }
            _history.Add(Current);
			HistoryIndex = _history.Count - 1;
		}

        [RelayCommand]
        void MoveUp()
        {
			Current = Current.Parent ?? throw new NullReferenceException();
			AddPage();
		}

        [RelayCommand]
        void MoveBack()
        {
            Current = _history[--HistoryIndex];
        }

        [RelayCommand]
        void MoveForward()
        {
			Current = _history[++HistoryIndex];
		}

        public void Navigate(NodeViewModelBase target)
        {
			Current = target;
			AddPage();
		}
	}
}
