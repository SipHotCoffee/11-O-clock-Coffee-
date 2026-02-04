using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.Visitors;
using System.Windows.Controls;
using System.Windows.Input;

namespace CG.Test.Editor.FrontEnd.Views
{
    public partial class MainWindow : CustomWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainViewModel ViewModel => (MainViewModel)DataContext;

        private void ObjectViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (ListViewItem)sender;
            if (ViewModel.SelectedFile is not null && item.Content is KeyValuePair<string, NodeViewModelBase> node)
            {
                node.Value.Visit(new NodeEditorVisitor(ViewModel.SelectedFile, true));
			}
        }

        private void ArrayViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
			var item = (ListViewItem)sender;
			if (ViewModel.SelectedFile is not null && item.Content is NodeViewModelBase node)
			{
				node.Visit(new NodeEditorVisitor(ViewModel.SelectedFile, false));
			}
		}
    }
}