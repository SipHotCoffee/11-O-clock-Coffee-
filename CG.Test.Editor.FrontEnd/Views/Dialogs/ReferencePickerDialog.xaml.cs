using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using DependencyPropertyToolkit;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public class TreeNodeViewModel(LinkedSchemaTypeBase typeFilter, NodeViewModelBase node)
    {
        public string Name { get; } = node.Name;

        public List<TreeNodeViewModel> Children { get; } = [.. node.Children.Where((node) => node is ArrayNodeViewModel || node is ObjectNodeViewModel)
                                                                            .Where((node) => node.Children.Any((node) => typeFilter.IsConvertibleFrom(node.Type)))
                                                                            .Select((node) => new TreeNodeViewModel(typeFilter, node)) ];
    }

    public partial class ReferencePickerDialog : Window
    {
        public ReferencePickerDialog()
        {
            InitializeComponent();
        }

        [DependencyProperty]
        public partial TreeNodeViewModel Root { get; set; }
    }
}
