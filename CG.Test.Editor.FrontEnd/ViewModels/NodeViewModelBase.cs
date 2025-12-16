using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public abstract partial class NodeViewModelBase(FileInstanceViewModel editor, NodeViewModelBase? parent) : ObservableObject
    {
        [ObservableProperty]
        private bool _hasChanges;

        partial void OnHasChangesChanged(bool oldValue, bool newValue)
        {
            Parent?.HasChanges = newValue;   
        }

        public FileInstanceViewModel Editor { get; } = editor;

        public NodeViewModelBase? Parent { get; } = parent;

        public string Name => Parent?.GetName(this) ?? "(Root)";

        public abstract SchemaTypeBase Type { get; }

        public abstract NodeViewModelBase Clone(NodeViewModelBase? parent);

        public abstract void SerializeTo(Utf8JsonWriter writer);

		protected abstract string GetName(NodeViewModelBase item);

		[RelayCommand]
		void Navigate()
		{
			Editor.Navigate(this);
		}
	}
}
