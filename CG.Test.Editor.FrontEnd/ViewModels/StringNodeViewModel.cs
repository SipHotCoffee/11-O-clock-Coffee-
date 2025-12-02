using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class StringNodeViewModel : NodeViewModelBase
    {
		[ObservableProperty]
		private string _value;

		public StringNodeViewModel(SchemaStringType type, string value)
		{
			Type  = type;
			Value = value;
		}

		public override SchemaStringType Type { get; }

		public override StringNodeViewModel Clone() => new(Type, Value);
    }
}
