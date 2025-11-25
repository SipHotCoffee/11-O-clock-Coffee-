using CG.Test.Editor.Models.Nodes;
using CG.Test.Editor.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Controls;

namespace CG.Test.Editor.Views
{
    public partial class EditorNodeItem : ObservableObject
    {
        public EditorNodeItem(EditorNodeItem? parent, string address, JsonNodeBase node, UserControl control)
        {
            Parent = parent;

            Address = address;
            Node    = node;
            Control = control;
        }

        public EditorNodeItem? Parent { get; }

        [ObservableProperty]
        private string _address;

        [ObservableProperty]  
        private JsonNodeBase _node;

        public UserControl Control { get; }

        public override string ToString() => Address;
    }

    public partial class NodeEditorView : UserControl
    {
        public NodeEditorView()
        {
            InitializeComponent();
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }
    }
}
