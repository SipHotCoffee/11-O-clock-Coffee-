using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<FileInstanceViewModel> OpenFiles { get; } = [];

        void NewFile(Window window)
        {
            
            var instance = new FileInstanceViewModel(window, );

            OpenFiles.Add(instance);
		}

        void OpenFile(Window window)
        {

        }

        [RelayCommand]
        void Exit(Window window)
        {
            window.Close();
        }
    }
}
