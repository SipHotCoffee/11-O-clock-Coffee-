using CG.Test.Editor.FrontEnd.Models;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class IntegerNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, SchemaIntegerType type, long value) : ValueNodeViewModelBase<long>(editor, parent, value)
    {
        public override SchemaIntegerType Type { get; } = type;

        public override IntegerNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Value);

        protected override string GetName(NodeViewModelBase item) => Value.ToString();

        public override void SerializeTo(Utf8JsonWriter writer)
        {
			writer.WriteNumberValue(Value);
        }
	}
}
