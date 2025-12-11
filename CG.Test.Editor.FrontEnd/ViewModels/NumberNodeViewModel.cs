using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class NumberNodeViewModel : NodeViewModelBase
    {
        [ObservableProperty]
        private double _value;

        public NumberNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, SchemaNumberType type, double value) : base(editor, parent)
        {
            Type  = type;
            Value = value;
        }

        partial void OnValueChanged(double oldValue, double newValue)
        {
            HasChanges = true;
        }

		public override SchemaNumberType Type { get; }

		public override NumberNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Value);

        protected override string GetName(NodeViewModelBase item) => string.Format("{0:F2}", Value);

        public override void SerializeTo(Utf8JsonWriter writer)
        {
            writer.WriteNumberValue(Value);
        }
    }
}
