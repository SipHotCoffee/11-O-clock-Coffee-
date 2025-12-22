using CG.Test.Editor.FrontEnd.ViewModels;
using DependencyPropertyToolkit;
using System.ComponentModel;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public partial class SearchArrayDialog : CustomWindow
    {
        public SearchArrayDialog()
        {
            InitializeComponent();
        }

        [DependencyProperty]
        public partial ICollectionView NodeCollection { get; set; }


        [DependencyProperty("")]
        public partial string SearchText { get; set; }

		[DependencyProperty]
		public partial NodeViewModelBase SelectedNode { get; set; }

        partial void OnSearchTextChanged(string oldValue, string newValue)
        {
			NodeCollection.Filter = (value) =>
			{
				if (value is NodeViewModelBase node)
				{
					if (node is StringNodeViewModel stringNode)
					{
						return stringNode.Value.Contains(newValue, StringComparison.CurrentCultureIgnoreCase);
					}
					else if (node is ObjectNodeViewModel objectNode &&
								objectNode.Type.TryGetProperty("name", out var property) &&
								objectNode.Nodes[property.Index].Value is StringNodeViewModel nameNode)
					{
						return nameNode.Value.Contains(newValue, StringComparison.CurrentCultureIgnoreCase);
					}
				}
				return false;
			};
		}

		private static void SearchTextChanged(SearchArrayDialog dialog, string searchText)
        {
            dialog.NodeCollection.Filter = (value) =>
            {
                if (value is NodeViewModelBase node)
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

 

		//private static void SearchTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		//{
		//	if (dependencyObject is SearchArrayDialog dialog && dialog.NodeCollection is not null)
		//	{
		//		dialog.NodeCollection.Filter = (value) =>
		//		{
		//			if (value is NodeViewModelBase node && e.NewValue is string searchText)
		//			{
		//				if (node is StringNodeViewModel stringNode)
		//				{
		//					return stringNode.Value.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
		//				}
		//				else if (node is ObjectNodeViewModel objectNode &&
		//						 objectNode.Type.TryGetProperty("name", out var property) &&
		//						 objectNode.Nodes[property.Index].Value is StringNodeViewModel nameNode)
		//				{
		//					return nameNode.Value.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
		//				}
		//			}
		//			return false;
		//		};
		//	}
		//}

		private void ArrayViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
		}
	}
}
