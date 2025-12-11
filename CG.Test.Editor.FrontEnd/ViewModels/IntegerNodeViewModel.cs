using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class IntegerNodeViewModel : NodeViewModelBase
    {
		[ObservableProperty]
		private long _value;

		public IntegerNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, SchemaIntegerType type, long value) : base(editor, parent)
		{
			Type  = type;
			Value = value;
		}

        partial void OnValueChanged(long oldValue, long newValue)
        {
			HasChanges = true;
        }

		public override SchemaIntegerType Type { get; }

		public override IntegerNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Value);

        protected override string GetName(NodeViewModelBase item) => Value.ToString();

        public override void SerializeTo(Utf8JsonWriter writer)
        {
			writer.WriteNumberValue(Value);
        }
    }
}
