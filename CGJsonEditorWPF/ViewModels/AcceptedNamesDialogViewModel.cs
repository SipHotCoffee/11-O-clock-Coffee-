using CG.Test.Editor.Json.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;

namespace CG.Test.Editor.ViewModels
{
    public partial class AcceptedNamesDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _defaultName;

        public ObservableCollection<string> AcceptedNames { get; } = [];

        [RelayCommand]
        public void InsertName(Window owner)
        {
            var newDialogViewModel = new StringValueDialogViewModel()
            {
                
                Text  = string.Empty,
                Label = $"Alternate Name for {DefaultName}",
            };

            var nameDialog = new StringValueDialogView()
            {
                Owner = owner,
                DataContext = newDialogViewModel
            };

            if (nameDialog.ShowDialog() ?? false)
            {
                AcceptedNames.Add(newDialogViewModel.Text);
            }
        }

        [RelayCommand]
        void DeleteNames(IEnumerable selectedItems)
        {
            var selectedNames = selectedItems.OfType<string>().ToList();
            foreach (var selectedName in selectedNames)
            {
                AcceptedNames.Remove(selectedName);
            }
        }

        [RelayCommand]
        static void Ok(Window window)
        {
            window.DialogResult = true;
            window.Close();
        }
    }
}
