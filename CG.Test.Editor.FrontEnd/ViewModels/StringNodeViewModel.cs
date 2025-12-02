using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class StringNodeViewModel : NodeViewModelBase
    {
		[ObservableProperty]
		private string _value;

		public StringNodeViewModel(NodeViewModelBase? parent, SchemaStringType type, string value) : base(parent)
		{
			Type  = type;
			Value = value;
		}

		public override SchemaStringType Type { get; }

		public override StringNodeViewModel Clone(NodeViewModelBase? parent) => new(parent, Type, Value);
    }
}
