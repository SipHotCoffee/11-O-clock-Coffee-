using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public struct Vector3([Range(-100, 100)] float x, [Range(-100, 100)] float y, [Range(-100, 100)]  float z);
    public struct Transformation(Vector3 position, Vector3 rotation, Vector3 scale);

    public class RootNode(Transformation transformation, bool booleanValue, string stringValue, string[] names, bool[] flags)
    {
        
    }

    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private FileInstanceViewModel? _selectedFile;

        public ObservableCollection<FileInstanceViewModel> OpenFiles { get; } = [];

        [RelayCommand]
        void NewFile(Window window)
        {
            var instance = new FileInstanceViewModel(window, typeof(RootNode).GetSchemaFromType(null).Visit(new NodeViewModelGeneratorVisitor(null)));
            OpenFiles.Add(instance);
            SelectedFile = instance;
		}

		[RelayCommand]
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
