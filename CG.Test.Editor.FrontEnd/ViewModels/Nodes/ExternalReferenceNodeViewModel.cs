using CG.Test.Editor.FrontEnd.Models.Types;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class ExternalReferenceNodeViewModel(NodeTree tree, NodeViewModelBase? parent, SchemaExternalReferenceType type, NodeViewModelBase? node) : ReferenceNodeViewModel(tree, parent, type, node)
    {
    }
}
