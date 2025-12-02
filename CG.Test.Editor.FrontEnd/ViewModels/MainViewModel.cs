using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public struct Vector3(float x, float y, float z);
    public struct Transformation(Vector3 position, Vector3 rotation, Vector3 scale);

    public class RootNode(Transformation transformation, bool booleanValue, string stringValue, string[] names)
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
            var instance = new FileInstanceViewModel(window, typeof(RootNode).GetSchemaFromType().Visit(new NodeViewModelGeneratorVisitor(null)));
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
