using CG.Test.Editor.FrontEnd.Models.Types;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class IntegerNodeViewModel(NodeTree tree, NodeViewModelBase? parent, SchemaIntegerType type, long value) : ValueNodeViewModelBase<long>(tree, parent, value)
    {
        public override SchemaIntegerType Type { get; } = type;

        public override IntegerNodeViewModel Clone(NodeViewModelBase? parent) => new(Tree, parent, Type, Value);

        protected override string GetName(NodeViewModelBase item) => Value.ToString();

        public override void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, ulong> referencedNodes)
        {
			writer.WriteNumberValue(Value);
        }
	}
}
