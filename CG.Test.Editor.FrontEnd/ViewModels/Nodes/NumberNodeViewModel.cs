using CG.Test.Editor.FrontEnd.Models.Types;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class NumberNodeViewModel(NodeTree tree, NodeViewModelBase? parent, SchemaNumberType type, double value) : ValueNodeViewModelBase<double>(tree, parent, value)
    {
        public override SchemaNumberType Type { get; } = type;

        public override NumberNodeViewModel Clone(NodeViewModelBase? parent) => new(Tree, parent, Type, Value);

        protected override string GetName(NodeViewModelBase item) => string.Format("{0:F2}", Value);

        public override void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, int> referencedNodes)
        {
            writer.WriteNumberValue(Value);
        }
    }
}
