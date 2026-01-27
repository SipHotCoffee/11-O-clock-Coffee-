using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using DependencyPropertyToolkit;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public class TreeNodeViewModel(LinkedSchemaTypeBase typeFilter, NodeViewModelBase node)
    {
        public NodeViewModelBase Node { get; } = node;

        public string Name { get; } = node.Name;

        public List<TreeNodeViewModel> Children { get; } = [.. node.Children.Where((node) => node is ArrayNodeViewModel || node is ObjectNodeViewModel)
                                                                            .Where((node) => node.Children.Any((node) => typeFilter.IsConvertibleFrom(node.Type)))
                                                                            .Select((node) => new TreeNodeViewModel(typeFilter, node)) ];
    }

    public partial class ReferencePickerDialog : CustomWindow
    {
		private readonly List<NodeViewModelBase> _history;

		public ReferencePickerDialog()
        {
            InitializeComponent();

            _history = [];

			AddressItems = [];

			HistoryIndex = 0;
		}

		[DependencyProperty]
		public partial int HistoryIndex { get; set; }

		[DependencyProperty]
		public partial bool IsBackButtonEnabled { get; set; }

		[DependencyProperty]
		public partial bool IsForwardButtonEnabled { get; set; }

		[DependencyProperty]
        public partial TreeNodeViewModel Root { get; set; }

        [DependencyProperty]
        public partial NodeViewModelBase Current { get; set; }

		[DependencyProperty]
		public partial ObservableCollection<NodeViewModelBase> AddressItems { get; set; }

		partial void OnRootChanged(TreeNodeViewModel oldValue, TreeNodeViewModel newValue)
        {
            Current = newValue.Node;
        }

		partial void OnCurrentChanged(NodeViewModelBase oldValue, NodeViewModelBase newValue)
		{
			AddressItems.Clear();
			for (var current = newValue; current is not null; current = current.Parent)
			{
				AddressItems.Insert(0, current);
			}
		}

		partial void OnHistoryIndexChanged(int oldValue, int newValue)
		{ 
			   IsBackButtonEnabled = newValue > 0;
			IsForwardButtonEnabled = newValue < _history.Count - 1;
		}

		private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Navigate(((TreeNodeViewModel)e.NewValue).Node);
		}

        private void ArrayViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			var item = (ListViewItem)sender;
			var node = (NodeViewModelBase)item.Content;
			Navigate(node);
			
		}

        private void ObjectViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			var item = (ListViewItem)sender;
			var (_, node) = (KeyValuePair<string, NodeViewModelBase>)item.Content;
			Navigate(node);
		}

		private void Navigate(NodeViewModelBase target)
		{
			Current = target;
			AddPage();
		}

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

		private void MoveForwardButton_Click(object sender, RoutedEventArgs e)
        {
			Current = _history[++HistoryIndex];
		}

        private void MoveBackButton_Click(object sender, RoutedEventArgs e)
        {
			Current = _history[--HistoryIndex];
		}

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
			Current = Current.Parent ?? throw new NullReferenceException();
			AddPage();
		}
	}
}
