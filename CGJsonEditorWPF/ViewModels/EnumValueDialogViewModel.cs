using CG.Test.Editor.Models.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace CG.Test.Editor.ViewModels
{
    public partial class EnumValueDialogViewModel : ObservableObject
    {
        //[ObservableProperty]
        //private IReadOnlyEnumMember _selectedMember;

        //[ObservableProperty]
        //private EditorEnumType? _enumType;

        [RelayCommand]
        void Ok(Window window)
        {
            window.DialogResult = true;
            window.Close();
        }
    }
}
