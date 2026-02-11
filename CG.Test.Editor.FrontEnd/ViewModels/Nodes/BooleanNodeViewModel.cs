using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class BooleanNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, LinkedSchemaBooleanType type, bool value) : ValueNodeViewModelBase<bool>(editor, parent, value)
	{
        public override LinkedSchemaBooleanType Type { get; } = type;

        public override BooleanNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Value);

        protected override string GetName(NodeViewModelBase item) => Value.ToString();

        public override void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, ulong> referencedNodes)
        {
            writer.WriteBooleanValue(Value);
        }

        public bool Equals(BooleanNodeViewModel? other) => Value == other?.Value;

        public override bool Equals(object? obj) => Equals(obj as BooleanNodeViewModel);

        public override int GetHashCode() => Value.GetHashCode();
    }
}
