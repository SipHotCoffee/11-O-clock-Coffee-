using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public partial class NumberNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, LinkedSchemaNumberType type, double value) : ValueNodeViewModelBase<double>(editor, parent, value)
    {
        public override LinkedSchemaNumberType Type { get; } = type;

        public override NumberNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Value);

        protected override string GetName(NodeViewModelBase item) => string.Format("{0:F2}", Value);

        public override void SerializeTo(Utf8JsonWriter writer)
        {
            writer.WriteNumberValue(Value);
        }
    }
}
