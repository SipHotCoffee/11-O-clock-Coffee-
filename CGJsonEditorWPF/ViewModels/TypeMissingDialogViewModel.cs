using CG.Test.Editor.Models.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace CG.Test.Editor.ViewModels
{
    public partial class TypeMissingDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _typeName = string.Empty;

        [ObservableProperty]
        private TypePickerDialogViewModel? _typePicker;

        [ObservableProperty]
        private JsonStructType? _selectedType;

        [RelayCommand]
        void Next(Window window)
        {
            SelectedType = TypePicker?.SelectedType as JsonStructType;
            window.DialogResult = true;
            window.Close();
        }
    }
}
