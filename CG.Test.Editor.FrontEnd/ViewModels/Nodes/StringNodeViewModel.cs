using CG.Test.Editor.FrontEnd.Models.Types;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class StringNodeViewModel(NodeTree tree, NodeViewModelBase? parent, SchemaStringType type, string value) : ValueNodeViewModelBase<string>(tree, parent, value)
    {
        public override SchemaStringType Type { get; } = type;

        public override StringNodeViewModel Clone(NodeViewModelBase? parent) => new(Tree, parent, Type, Value);

		protected override string GetName(NodeViewModelBase item) => $"\"{Value}\"";

        public override void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, ulong> referencedNodes)
        {
			writer.WriteStringValue(Value);
        }
    }
}
