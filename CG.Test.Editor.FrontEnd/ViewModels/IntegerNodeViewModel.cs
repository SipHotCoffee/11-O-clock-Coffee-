using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class IntegerNodeViewModel : NodeViewModelBase
    {
		[ObservableProperty]
		private long _value;

		public IntegerNodeViewModel(NodeViewModelBase? parent, SchemaIntegerType type, long value) : base(parent)
		{
			Type  = type;
			Value = value;
		}

		public override SchemaIntegerType Type { get; }

		public override IntegerNodeViewModel Clone(NodeViewModelBase? parent) => new(parent, Type, Value);
    }
}
