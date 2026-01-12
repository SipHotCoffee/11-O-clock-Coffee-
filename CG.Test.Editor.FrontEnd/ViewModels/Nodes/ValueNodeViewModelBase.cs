using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public abstract partial class ValueNodeViewModelBase<TValue> : NodeViewModelBase, IEquatable<ValueNodeViewModelBase<TValue>> where TValue : IEquatable<TValue>
    {
		[ObservableProperty]
		private TValue _value;

		public ValueNodeViewModelBase(FileInstanceViewModel editor, NodeViewModelBase? parent, TValue value) : base(editor, parent)
		{
			Value = value;
		}

		partial void OnValueChanged(TValue? oldValue, TValue newValue)
		{
			HasChanges = true;
		}

		public bool Equals(ValueNodeViewModelBase<TValue>? other) => other is not null && Value.Equals(other.Value);

		public override bool Equals(object? obj) => Equals(obj as ValueNodeViewModelBase<TValue>);

		public override int GetHashCode() => Value.GetHashCode();
    }
}
