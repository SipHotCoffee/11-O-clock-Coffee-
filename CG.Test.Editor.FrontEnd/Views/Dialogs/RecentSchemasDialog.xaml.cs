using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DependencyPropertyToolkit;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
	public partial class RecentSchemaViewModel(RecentSchemasDialog dialog, string name) : ObservableObject, IEquatable<RecentSchemaViewModel>
	{
		private readonly RecentSchemasDialog _dialog = dialog;

		public string FileName { get; } = name;

		[RelayCommand]
		async Task Delete()
		{
			_dialog.RecentSchemas.Remove(this);
			await _dialog.SaveRecentSchemas();
		}

        public bool Equals(RecentSchemaViewModel? other) => FileName == other?.FileName;

        public override bool Equals(object? obj) => Equals(obj as RecentSchemaViewModel);

		public override int GetHashCode() => FileName.GetHashCode();
    }

    public partial class RecentSchemasDialog : CustomWindow
    {
		private readonly SaveInfo _currentSaveInfo;

		public RecentSchemasDialog(SaveInfo currentSaveInfo)
        {
            InitializeComponent();

			_currentSaveInfo = currentSaveInfo;
			RecentSchemas = [.. currentSaveInfo!.RecentSchemas.Where(File.Exists).Select((name) => new RecentSchemaViewModel(this, name)).ToHashSet()];

		}

		[DependencyProperty]
		public partial ObservableCollection<RecentSchemaViewModel> RecentSchemas { get; set; }

        [DependencyProperty]
        public partial RecentSchemaViewModel SelectedSchema { get; set; }

		public async Task SaveRecentSchemas()
		{
			_currentSaveInfo.RecentSchemas = RecentSchemas.Select((viewModel) => viewModel.FileName);
			await _currentSaveInfo.SaveAsync();
		}

		private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
			_browseButton.IsEnabled = false;
			
            var openFileDialog = new OpenFileDialog
			{
				Filter = "Json Schema files (*.json)|*.json",
				InitialDirectory = @"C:\Database\Schemas"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				var recentSchema = new RecentSchemaViewModel(this, openFileDialog.FileName);
				RecentSchemas.Add(recentSchema);
                SelectedSchema = recentSchema;

				await SaveRecentSchemas();

				DialogResult = true;
				Close();
			}

			_browseButton.IsEnabled = true;
		}

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
		}
	}
}
