using CG.Test.Editor.Models.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace CG.Test.Editor.ViewModels
{
    public partial class TypePickerDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _includeAbstract;

        [ObservableProperty]
        private Type? _baseType;

        [ObservableProperty]
        private JsonTypeBase? _selectedType;

        public ObservableCollection<JsonTypeBase> Types { get; } = [];

        partial void OnBaseTypeChanged(Type? oldValue, Type? newValue)
        {
            Types.Clear();
            if (newValue is not null)
            {
                var types = IncludeAbstract ? newValue.EnumerateImplementedDerivedTypes() : newValue.EnumerateDerivedTypes();

                foreach (var type in types)
                {
                    Types.Add(JsonTypeBase.Get(type));
                }
            }
        }
    }
}
