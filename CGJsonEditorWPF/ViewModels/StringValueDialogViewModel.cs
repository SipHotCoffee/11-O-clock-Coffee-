using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace CG.Test.Editor.ViewModels
{
    public partial class StringValueDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _text;

        [ObservableProperty]
        private string _label;

        public StringValueDialogViewModel()
        {
            Text  = string.Empty;
            Label = string.Empty;
        }

        [RelayCommand]
        void Ok(Window window)
        {
            window.DialogResult = true;
            window.Close();
        }
    }
}
