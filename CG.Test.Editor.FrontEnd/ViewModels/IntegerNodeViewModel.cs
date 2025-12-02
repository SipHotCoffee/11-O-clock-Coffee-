using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class IntegerNodeViewModel : NodeViewModelBase
    {
		[ObservableProperty]
		private long _value;

		public IntegerNodeViewModel(SchemaIntegerType type, long value)
		{
			Type  = type;
			Value = value;
		}

		public override SchemaIntegerType Type { get; }

		public override IntegerNodeViewModel Clone() => new(Type, Value);
    }
}
