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
        private const string SAVE_FILE_NAME = "save.json";

        public RecentSchemasDialog()
        {
            InitializeComponent();
        }

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
            if (!File.Exists(SAVE_FILE_NAME))
            {
                await using var creationStream = File.Create(SAVE_FILE_NAME);
				await JsonSerializer.SerializeAsync(creationStream, new SaveInfo()
				{
					RecentSchemas = []
				});
			}

            await using var stream = File.OpenRead(SAVE_FILE_NAME);
			var saveInfo = await JsonSerializer.DeserializeAsync<SaveInfo>(stream);
            RecentSchemas = [..saveInfo!.RecentSchemas.Where(File.Exists).Select((name) => new RecentSchemaViewModel(this, name)).ToHashSet()];
		}

		[DependencyProperty]
		public partial ObservableCollection<RecentSchemaViewModel> RecentSchemas { get; set; }

        [DependencyProperty]
        public partial RecentSchemaViewModel SelectedSchema { get; set; }

		public async Task SaveRecentSchemas()
		{
			await using var stream = File.Create(SAVE_FILE_NAME);
			await JsonSerializer.SerializeAsync(stream, new SaveInfo()
			{
				RecentSchemas = RecentSchemas.Select((viewModel) => viewModel.FileName)
			});
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
