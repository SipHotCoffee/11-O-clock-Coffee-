using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

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

		public override SchemaStringType Type { get; }

		public override StringNodeViewModel Clone(NodeViewModelBase? parent) => new(Editor, parent, Type, Value);

		protected override string GetName(NodeViewModelBase item) => $"\"{Value}\"";
    }
}
