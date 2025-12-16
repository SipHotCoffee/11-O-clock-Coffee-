using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public partial class RecentSchemasDialog : CustomWindow
    {
        private const string SAVE_FILE_NAME = "save.json";

        public static readonly DependencyProperty  RecentSchemasProperty = DependencyProperty.Register(nameof(RecentSchemas), typeof(HashSet<string>), typeof(RecentSchemasDialog));
        public static readonly DependencyProperty SelectedSchemaProperty = DependencyProperty.Register(nameof(SelectedSchema), typeof(string), typeof(RecentSchemasDialog));

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
            RecentSchemas = [.. saveInfo!.RecentSchemas.Where(File.Exists)];
		}

		public HashSet<string> RecentSchemas
        {
            get => (HashSet<string>)GetValue(RecentSchemasProperty);
            set => SetValue(RecentSchemasProperty, value);
        }

        public string SelectedSchema
        {
            get => (string)GetValue(SelectedSchemaProperty);
            set => SetValue(SelectedSchemaProperty, value);
        }

		private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
			_browseButton.IsEnabled = false;
			
            var openFileDialog = new OpenFileDialog
			{
				Filter = "Json Schema files (*.json)|*.json"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				RecentSchemas.Add(openFileDialog.FileName);
                SelectedSchema = openFileDialog.FileName;

                await using var stream = File.Create(SAVE_FILE_NAME);
				await JsonSerializer.SerializeAsync(stream, new SaveInfo()
                {
                    RecentSchemas = RecentSchemas
                });

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
