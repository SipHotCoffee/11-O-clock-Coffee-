using CG.Test.Editor.FrontEnd.Models.Types;
using CG.Test.Editor.FrontEnd.ViewModels;
using DependencyPropertyToolkit;
using Microsoft.Win32;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public class FileIncludeInfo
    {
        public required FileInfo File { get; init; }
        
        public required SchemaTypeBase Type { get; init; }
    }

    public partial class IncludeFilesDialog : CustomWindow
    {
        private HashSet<string> _includedFileNames;

        public IncludeFilesDialog()
        {
            InitializeComponent();

            _includedFileNames = [];

            IncludedFiles = [];
        }

        [DependencyProperty]
        public partial ObservableCollection<FileInfo> IncludedFiles { get; set; }

        partial void OnIncludedFilesChanged(ObservableCollection<FileInfo> oldValue, ObservableCollection<FileInfo> newValue)
        {
            _includedFileNames = [.. newValue.Select((includedFile) => includedFile.FullName)];
        }

        private async void InsertButton_Click(object sender, RoutedEventArgs e)
        {
			var openFileDialog = new OpenFileDialog()
			{
				Filter = "Json files (*.json)|*.json",
				InitialDirectory = @"C:\Database",
				Multiselect = true,
			};

			if (openFileDialog.ShowDialog() == true)
			{
                foreach (var fileName in openFileDialog.FileNames)
                {
                    if (!_includedFileNames.Add(fileName))
                    {
                        continue;
                    }

                    var file = new FileInfo(fileName);
                    IncludedFiles.Add(file);
                }
			}
		}

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var includedFile in _includedFilesListView.SelectedItems.OfType<FileInfo>())
            {
                IncludedFiles.Remove(includedFile);
                _includedFileNames.Remove(includedFile.FullName);
            }
		}

        private void IncludedFilesListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _deleteButton.IsEnabled = _includedFilesListView.SelectedItems.Count > 0;
		}

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
		}
	}
}
