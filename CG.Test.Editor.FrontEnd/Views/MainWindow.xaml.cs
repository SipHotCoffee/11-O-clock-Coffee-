using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.Visitors;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CG.Test.Editor.FrontEnd.Views
{
    public partial class MainWindow : CustomWindow
    {
        public MainWindow(IEnumerable<FileInfo> files)
        {
            DataContext = new MainViewModel(files);
            InitializeComponent();
        }

        public MainViewModel ViewModel => (MainViewModel)DataContext;

        private void ObjectViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (ListViewItem)sender;
            if (ViewModel.SelectedFile is not null && item.Content is KeyValuePair<string, NodeViewModelBase> node)
            {
                node.Value.Visit(new NodeEditorVisitor(true));
			}
        }

        private void ArrayViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
			var item = (ListViewItem)sender;
			if (ViewModel.SelectedFile is not null && item.Content is NodeViewModelBase node)
			{
				node.Visit(new NodeEditorVisitor(false));
			}
		}

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !await ViewModel.CloseAllAsync();
		}

        private async void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await ViewModel.LoadAsync(this);
		}
	}
}