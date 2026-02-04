using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using DependencyPropertyToolkit;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

	public class NodeContainsMatchingType : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var node = (NodeViewModelBase?)values[0];
			var type = (LinkedSchemaTypeBase?)values[1];

			if(node is null || type is null)
			{
				return false;
			}

			return type.IsConvertibleFrom(node.Type) || node.AllChildren.Any((child) => type.IsConvertibleFrom(child.Type));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class SelectedNodeHasMatchingType : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values[0] is NodeViewModelBase node && values[1] is LinkedSchemaTypeBase type)
			{
				return type.IsConvertibleFrom(node.Type);
			}
			return false;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class NodeFromPairConverter : IValueConverter
	{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
			if (value is null)
			{
				return default(KeyValuePair<string, NodeViewModelBase>);
			}

			var node = (NodeViewModelBase)value;
			return new KeyValuePair<string, NodeViewModelBase>(node.Name, node);
		}

		public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is null)
			{
				return null;
			}

			var (_, node) = (KeyValuePair<string, NodeViewModelBase>)value;
			return node;
		}
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
		public partial LinkedSchemaTypeBase FilterType { get; set; }

		[DependencyProperty]
		public partial int HistoryIndex { get; set; }

		[DependencyProperty]
		public partial bool IsBackButtonEnabled { get; set; }

		[DependencyProperty]
		public partial bool IsForwardButtonEnabled { get; set; }

		[DependencyProperty]
        public partial TreeNodeViewModel Root { get; set; }

        [DependencyProperty]
        public partial NodeViewModelBase? CurrentNode { get; set; }

		[DependencyProperty(null)]
		public partial NodeViewModelBase? SelectedNode { get; set; }

		[DependencyProperty]
		public partial ObservableCollection<NodeViewModelBase> AddressItems { get; set; }

		partial void OnRootChanged(TreeNodeViewModel oldValue, TreeNodeViewModel newValue)
        {
            CurrentNode = newValue.Node;
        }

        partial void OnSelectedNodeChanged(NodeViewModelBase? oldValue, NodeViewModelBase? newValue)
        {
            if (newValue is not null)
			{
				CurrentNode = newValue.Parent;
			}
        }

		partial void OnCurrentNodeChanged(NodeViewModelBase? oldValue, NodeViewModelBase? newValue)
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
			if (target.AllChildren.Any((child) => FilterType.IsConvertibleFrom(child.Type)))
			{
				CurrentNode = target;
				AddPage();
			}
		}

		private void AddPage()
		{
			if (CurrentNode is null)
			{
				return;
			}

			var nextIndex = HistoryIndex + 1;
			if (nextIndex < _history.Count)
			{
				_history.RemoveRange(nextIndex, _history.Count - nextIndex);
			}
			_history.Add(CurrentNode);
			HistoryIndex = _history.Count - 1;
		}

		private void MoveForwardButton_Click(object sender, RoutedEventArgs e)
        {
			CurrentNode = _history[++HistoryIndex];
		}

        private void MoveBackButton_Click(object sender, RoutedEventArgs e)
        {
			CurrentNode = _history[--HistoryIndex];
		}

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
			CurrentNode = CurrentNode?.Parent ?? throw new NullReferenceException();
			AddPage();
		}

        private void AddressItemButton_Click(object sender, RoutedEventArgs e)
        {
			var control = (FrameworkElement)sender;
			var node = (NodeViewModelBase)control.Tag;
			Navigate(node);
		}

		private void ArrayListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var listView = (ListView)sender;

			if (listView.SelectedItem is NodeViewModelBase node)
			{
				SelectedNode = node;
			}
			else
			{
				SelectedNode = null;
			}
		}

		private void ObjectListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			var listView = (ListView)sender;

			if (listView.SelectedItem is KeyValuePair<string, NodeViewModelBase> pair)
			{
				SelectedNode = pair.Value;
			}
			else
			{
				SelectedNode = null;
			}
		}

        private void SelectedButton_Click(object sender, RoutedEventArgs e)
        {
			DialogResult = true;
			Close();
		}
	}
}
