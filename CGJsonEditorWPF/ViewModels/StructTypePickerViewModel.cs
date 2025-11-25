using CG.Test.Editor.Models.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace CG.Test.Editor.ViewModels
{
    public partial class StructTypePickerViewModel : ObservableObject
    {
        [ObservableProperty]
        private JsonStructType? _baseType;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private JsonStructType? _selectedType;

        public StructTypePickerViewModel()
        {
            VisibleStructTypes = [];

            SearchText = string.Empty;
        }

        public ObservableCollection<JsonStructType> VisibleStructTypes { get; }

        partial void OnBaseTypeChanged(JsonStructType? oldValue, JsonStructType? newValue)
        {
            VisibleStructTypes.Clear();
            if (newValue is not null)
            {
                VisibleStructTypes.Add(newValue);
                foreach (var derivedType in newValue.DerivedTypes)
                {
                    VisibleStructTypes.Add(derivedType);
                }

                SelectedType = BaseType;
            }
        }

        partial void OnSearchTextChanged(string? oldValue, string newValue)
        {
            if (newValue == string.Empty && BaseType is not null)
            {
                foreach (var derivedType in BaseType.DerivedTypes)
                {
                    VisibleStructTypes.Add(derivedType);
                }
            }

            var index = 0;
            while (index < VisibleStructTypes.Count)
            {
                if (!VisibleStructTypes[index].Name.Contains(newValue, StringComparison.InvariantCulture))
                {
                    VisibleStructTypes.RemoveAt(index);
                }
                else
                {
                    ++index;
                }
            }
        }
    }
}
