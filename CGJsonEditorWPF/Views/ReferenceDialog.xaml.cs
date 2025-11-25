using CG.Test.Editor.Models.Nodes;
using CG.Test.Editor.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CG.Test.Editor.Json.Dialogs
{
    public partial class ReferenceDialog : CustomWindow
    {

        public ReferenceDialog()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var treeView = (TreeView)sender;
            var viewModel = (ReferenceDialogViewModel)DataContext;
            var item = (EditorNodeTreeNode)treeView.SelectedItem;
            viewModel.SelectedNode = item.Node;
        }
    }
}
