using CG.Test.Editor.FrontEnd.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public partial class SearchArrayDialog : CustomWindow
    {
        public static readonly DependencyProperty     SearchTextProperty = DependencyProperty.Register(nameof(SearchText)    , typeof(string),            typeof(SearchArrayDialog), new PropertyMetadata(string.Empty, SearchTextChanged));
        public static readonly DependencyProperty NodeCollectionProperty = DependencyProperty.Register(nameof(NodeCollection), typeof(ICollectionView),   typeof(SearchArrayDialog));
        public static readonly DependencyProperty   SelectedNodeProperty = DependencyProperty.Register(nameof(SelectedNode)  , typeof(NodeViewModelBase), typeof(SearchArrayDialog));

        public SearchArrayDialog()
        {
            InitializeComponent();
        }

        public ICollectionView NodeCollection
        {
            get => (ICollectionView)GetValue(NodeCollectionProperty);
            set => SetValue(NodeCollectionProperty, value);
        }


		public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public NodeViewModelBase SelectedNode
        {
            get => (NodeViewModelBase)GetValue(SelectedNodeProperty);
            set => SetValue(SelectedNodeProperty, value);
        }

		private static void SearchTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is SearchArrayDialog dialog && dialog.NodeCollection is not null)
            {
                dialog.NodeCollection.Filter = (value) =>
                {
                    if (value is NodeViewModelBase node && e.NewValue is string searchText)
                    {
                        if (node is StringNodeViewModel stringNode)
                        {
                            return stringNode.Value.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
                        }
                        else if (node is ObjectNodeViewModel objectNode &&
                                 objectNode.Type.TryGetProperty("name", out var property) && 
                                 objectNode.Nodes[property.Index].Value is StringNodeViewModel nameNode)
                        {
							return nameNode.Value.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
						}
                    }
                    return false;
                };
			}
        }

		private void ArrayViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
		}
	}
}
