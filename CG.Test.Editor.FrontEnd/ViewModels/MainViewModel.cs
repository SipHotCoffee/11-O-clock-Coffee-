using CG.Test.Editor.FrontEnd.Models.Types;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using CG.Test.Editor.FrontEnd.Visitors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Input;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public class SchemaParsingMessageComparer : IEqualityComparer<SchemaParsingMessage>
    {
        public bool Equals(SchemaParsingMessage x, SchemaParsingMessage y)
        {
            return x.Message == y.Message;
        }

        public int GetHashCode(SchemaParsingMessage obj)
        {
            return obj.Message.GetHashCode();
        }
    }

	public class CollectionLogger<TItem>(ICollection<TItem> targetCollection) : ILogger<TItem>
    {
        private readonly ICollection<TItem> _targetCollection = targetCollection;

        public void Log(TItem item)
        {
            _targetCollection.Add(item);
        }
    }

	public enum AppStyleNodeType
	{
		Style,
		ColorScheme
	}

	public partial class AppStyleNode : ObservableObject
	{
		public required AppStyleNode? Parent { get; init; }

		public required string Name { get; init; }

		public required AppStyleNodeType Type { get; init; }

		public ObservableCollection<AppStyleNode> Children { get; } = [];

		[RelayCommand]
		async Task Apply()
		{
			if (Type == AppStyleNodeType.ColorScheme)
			{
				var saveInfo = await SaveInfo.LoadAsync();
				saveInfo.LastStyleName = Parent!.Name;
				saveInfo.LastColorSchemeName = Name;

				var appResources = Application.Current.Resources;

				appResources.MergedDictionaries.Clear();
				appResources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri($"Resources/Styles/{Parent!.Name}/ColorSchemes/{Name}.xaml", UriKind.Relative) });
				appResources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri($"Resources/Styles/{Parent!.Name}/Style.xaml", UriKind.Relative) });

				await saveInfo.SaveAsync();
			}
		}
	}

    public partial class MainViewModel : ObservableObject
    {
		private readonly IEnumerable<FileInfo> _filesToOpen;

        [ObservableProperty]
        private FileInstanceViewModel? _selectedFile;

        [ObservableProperty]
        private bool _isAltDown;

        public ObservableCollection<FileInstanceViewModel> OpenFiles { get; }

		public ObservableCollection<AppStyleNode> Styles { get; }

        public MainViewModel(IEnumerable<FileInfo> filesToOpen)
        {
			_filesToOpen = filesToOpen;

			InputManager.Current.PreProcessInput += (sender, e) => IsAltDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);

			OpenFiles = [];

			var styleNodes = new Dictionary<string, AppStyleNode>();

			foreach (var resourceDictionary in EnumerateStyleDictionaries())
			{
				var styleName = (string)resourceDictionary["StyleName"];
				if (!styleNodes.TryGetValue(styleName, out var styleNode))
				{
					styleNode = new AppStyleNode() { Parent = null, Name = styleName, Type = AppStyleNodeType.Style };
					styleNodes.Add(styleName, styleNode);
				}
				
				var styleNodeType = (string)resourceDictionary["NodeType"];
				if (styleNodeType == "ColorScheme")
				{
					var colorSchemeName = (string)resourceDictionary["ColorSchemeName"];

					styleNode.Children.Add(new AppStyleNode()
					{
						Parent = styleNode,
						Name = colorSchemeName,
						Type = AppStyleNodeType.ColorScheme,
					});
				}
			}

			Styles = new(styleNodes.Values);
		}

		private static IEnumerable<ResourceDictionary> EnumerateStyleDictionaries()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var assemblyName = assembly.GetName().Name;

			var resourceNames = assembly.GetManifestResourceNames();

			var gResourceName = resourceNames.FirstOrDefault(n => n.EndsWith(".g.resources"));

			if (gResourceName is null)
			{
				yield break;
			}

			using var stream = assembly.GetManifestResourceStream(gResourceName);
			using var reader = new ResourceReader(stream!);

			foreach (var entry in reader.OfType<DictionaryEntry>())
			{
				var resourceKey = (string)entry.Key;

				if (resourceKey.EndsWith(".xaml"))
				{
					var xamlPath = resourceKey.Replace(".baml", ".xaml");
					yield return new ResourceDictionary() { Source = new Uri($"/{assemblyName};component/{xamlPath}", UriKind.Relative) };
				}
			}
		}

		public static async Task<SchemaTypeBase?> LoadSchema(FileInfo file, ILogger<SchemaParsingMessage> logger)
		{
			await using var stream = file.OpenRead();
			return await SchemaTypeBase.LoadFromStream(stream, logger);
		}

		public static async Task<FileInfo?> SelectSchemaFile(Window window)
        {
			ArgumentNullException.ThrowIfNull(window, nameof(window));

			var saveInfo = await SaveInfo.LoadAsync();

            var recentSchemasDialog = new RecentSchemasDialog(saveInfo);

			if (recentSchemasDialog.ShowDialog() == true)
			{
				return new FileInfo(recentSchemasDialog.SelectedSchema.FileName);
			}

            return null;
		}

		public async Task LoadAsync(Window window)
		{
			var saveInfo = await SaveInfo.LoadAsync();
			if (saveInfo.LastStyleName != string.Empty && saveInfo.LastColorSchemeName != string.Empty)
			{
				var appResources = Application.Current.Resources;

				appResources.MergedDictionaries.Clear();
				appResources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri($"Resources/Styles/{saveInfo.LastStyleName}/ColorSchemes/{saveInfo.LastColorSchemeName}.xaml", UriKind.Relative) });
				appResources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri($"Resources/Styles/{saveInfo.LastStyleName}/Style.xaml", UriKind.Relative) });
			}

			if (!_filesToOpen.Any())
			{
				return;
			}

			await using var streams = await OpenFilesAsync(window, _filesToOpen);
		}

        public async Task<IAsyncDisposableCollection> OpenFilesAsync(Window window, IEnumerable<FileInfo> files) 
			=> new AsyncDisposableCollection<Stream[]>(await Task.WhenAll(files.Select((file) => OpenFileAsync(window, file))));

        private async Task<Stream> OpenFileAsync(Window window, FileInfo file)
		{
			var stream = file.OpenRead();
			try
			{
				var instance = new FileInstanceViewModel(this, file, window);

				var messages = new HashSet<string>();
				var logger = new CollectionLogger<string>(messages);

				var loadedFile = await NodeViewModelBase.ParseFromStream(window, instance, file, stream, logger);

				foreach (var message in messages)
				{
					window.ShowMessage(message);
				}

				if (loadedFile is null)
				{
					return stream;
				}

				instance.HasChanges = false;
				instance.SchemaFile = loadedFile.SchemaFile;
				instance.Root = loadedFile.RootNode;
				OpenFiles.Add(instance);
				SelectedFile = instance;
			}
			catch (Exception exception)
			{
				window.ShowMessage(exception.ToString());
			}
			return stream;
		}

		public async Task<bool> CloseAllAsync()
        {
			while (OpenFiles.Count > 0)
			{
				var file = OpenFiles[^1];
				if (!await file.CloseAsync())
				{
					return false;
				}
			}
            return true;
        }

		[RelayCommand]
        async Task NewFile(Window window)
        {
			var instance = new FileInstanceViewModel(this, null, window);
            var schemaFile = await SelectSchemaFile(window);
			if (schemaFile is null)
			{
				return;
			}

			var messages = new HashSet<SchemaParsingMessage>(10, new SchemaParsingMessageComparer());
			var logger = new CollectionLogger<SchemaParsingMessage>(messages);

			var schemaType = await LoadSchema(schemaFile, logger);

			foreach (var schemaMessage in messages)
			{
				window.ShowMessage(schemaMessage.Message);
			}

            if (schemaType is not null)
            {
				var tree = new NodeTree(instance.File, instance);
				instance.Root = schemaType.Visit(new NodeViewModelGeneratorVisitor(window, tree, null, null));
				instance.SchemaFile = schemaFile;
                OpenFiles.Add(instance);
                SelectedFile = instance;
            }
		}

		[RelayCommand]
		async Task OpenFile(Window window)
		{
			var openFileDialog = new OpenFileDialog()
			{
				Filter = "Json files (*.json)|*.json",
				InitialDirectory = @"C:\Database",
				Multiselect = true,
			};

			if (openFileDialog.ShowDialog() == true)
			{
				await using var streams = await OpenFilesAsync(window, openFileDialog.FileNames.Select((fileName) => new FileInfo(fileName)));
			}
		}

		[RelayCommand]
        async Task SaveFile()
        {
            if (SelectedFile is not null)
            {
                await SelectedFile.SaveAsync();
            }
        }

        [RelayCommand]
        async Task SaveFileAs()
        {
            if (SelectedFile is not null)
            {
                await SelectedFile.SaveAsAsync();
            }
        }


		[RelayCommand]
		async Task SaveAll()
		{
            await Task.WhenAll(OpenFiles.Select((file) => file.SaveAsync()));
		}

		[RelayCommand]
        void Exit(Window window)
        {
            window.Close();
        }
    }
}
