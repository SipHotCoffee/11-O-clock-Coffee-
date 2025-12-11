using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class StringNodeViewModel : NodeViewModelBase
    {
		[ObservableProperty]
		private string _value;

		public StringNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, SchemaStringType type, string value) : base(editor, parent)
		{
			Type  = type;
			Value = value;
		}

        partial void OnValueChanged(string? oldValue, string newValue)
        {
            HasChanges = true;
        }

		public override SchemaStringType Type { get; }

		public override StringNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Value);

		protected override string GetName(NodeViewModelBase item) => $"\"{Value}\"";

        public override void SerializeTo(Utf8JsonWriter writer)
        {
			writer.WriteStringValue(Value);
        }
    }
}
