using CG.Test.Editor.Models.Nodes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows;

namespace CG.Test.Editor.ViewModels
{
    public partial class ArrayElementSearchViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _searchText;

        [ObservableProperty]
        private JsonNodeBase? _selectedNode;

        [ObservableProperty]
        private ICollectionView? _elements;

        partial void OnSearchTextChanged(string? value)
        {
            if (Elements is null || value is null)
            {
                return;
            }

            Elements.Filter = (node) =>
            {
                var upperValue = value.ToUpper();

                if (node is JsonValueNode valueNode)
                {
                    if (valueNode.Value is string stringValue)
                    {
                        return stringValue.ToUpper().Contains(upperValue);
                    }
                    else if (valueNode.Value is IConvertible convertibleValue)
                    {
                        return (convertibleValue.ToString() ?? string.Empty).ToUpper().Contains(upperValue);
                    }
                }
             
                if (node is JsonObjectNode objectNode && objectNode.TryGetValue("name", out var nameNode) && nameNode is JsonValueNode nameValue && nameValue.Value is string name)
                {
                    return name.ToUpper().Contains(upperValue);
                }

                return false;
            };
        }

        [RelayCommand]
        public void Navigate(Window window)
        {
            window.DialogResult = true;
            window.Close();
        }
    }
}
