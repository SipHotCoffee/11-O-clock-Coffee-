using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CG.Test.Editor.FrontEnd.Views;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using CG.Test.Editor.FrontEnd.Visitors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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

    public partial class MainViewModel : ObservableObject
    {
		private readonly IEnumerable<FileInfo> _filesToOpen;

        [ObservableProperty]
        private FileInstanceViewModel? _selectedFile;

        [ObservableProperty]
        private bool _isAltDown;

        public ObservableCollection<FileInstanceViewModel> OpenFiles { get; } = [];

        public MainViewModel(IEnumerable<FileInfo> filesToOpen)
        {
			_filesToOpen = filesToOpen;

			InputManager.Current.PreProcessInput += (sender, e) => IsAltDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Alt);
		}

        private static async Task<LinkedSchemaTypeBase?> LoadSchema(Window window)
        {
			ArgumentNullException.ThrowIfNull(window, nameof(window));

            var recentSchemasDialog = new RecentSchemasDialog();

			if (recentSchemasDialog.ShowDialog() == true)
			{
				try
				{
					await using var stream = File.OpenRead(recentSchemasDialog.SelectedSchema.FileName);
					var node = await JsonNode.ParseAsync(stream);

					var messages = new HashSet<SchemaParsingMessage>(10, new SchemaParsingMessageComparer());
					var logger = new CollectionLogger<SchemaParsingMessage>(messages);

                    if (node is JsonObject objectNode && objectNode.TryGetPropertyValue("$defs", out var defsNode) && defsNode is JsonObject defsObjectNode)
                    {
                        var types = new Dictionary<string, LinkedSchemaTypeBase>();
						defsObjectNode.TryParseSchemaDefinitions(logger, types);
                        if (objectNode.TryParseLinkedSchemaType(logger, types, out var type))
                        {
							return type;
						}
					}

					foreach (var message in messages)
					{
						window.ShowMessage(message.Message);
					}
				}
				catch (Exception exception)
				{
					window.ShowMessage(exception.ToString());
				}
			}
            return null;
		}

		public async Task LoadAsync(Window window)
		{
			if (!_filesToOpen.Any())
			{
				return;
			}

			var schemaType = await LoadSchema(window);
			if (schemaType is null)
			{
				return;
			}

			await using var streams = await OpenFilesAsync(window, schemaType, _filesToOpen);
		}

        public async Task<AsyncDisposableCollection> OpenFilesAsync(Window window, LinkedSchemaTypeBase schemaType, IEnumerable<FileInfo> files) 
			=> new AsyncDisposableCollection(await Task.WhenAll(files.Select((file) => OpenFileAsync(window, schemaType, file))));

        private async Task<Stream> OpenFileAsync(Window window, LinkedSchemaTypeBase schemaType, FileInfo file)
		{
			var stream = file.OpenRead();
			try
			{
				var fileNode = await JsonNode.ParseAsync(stream);

				var fileObjectNode = fileNode!.AsObject();

				var referencePathsArrayNode = fileObjectNode["referencePaths"]!.AsArray();

				var paths = new Dictionary<ulong, NodePath>();

				foreach (var pathNode in referencePathsArrayNode)
				{
					var id = pathNode!["id"]!.GetValue<ulong>();

					var pathElementsArrayNode = pathNode["path"]!.AsArray();

					var path = NodePath.Root;

					foreach (var pathElementValueNode in pathElementsArrayNode.OfType<JsonValue>())
					{
						switch (pathElementValueNode.GetValueKind())
						{
							case JsonValueKind.String:
								path = path.GetChild(new NameIdentifier(pathElementValueNode.GetValue<string>()));
								break;
							case JsonValueKind.Number:
								path = path.GetChild(new IndexIdentifier(pathElementValueNode.GetValue<int>()));
								break;
						}
					}

					paths.Add(id, path);
				}

				var instance = new FileInstanceViewModel(this, file, window);

				var contentNode = fileObjectNode["content"];

				var messages = new List<NodeParsingMessage>();
				var logger = new CollectionLogger<NodeParsingMessage>(messages);

				var referenceNodesToAssign = new Dictionary<ulong, List<ReferenceNodeViewModel>>();
				var rootNodeViewModel = schemaType!.Visit(new NodeParserVisitor(instance, NodePath.Root, referenceNodesToAssign, null, logger, contentNode));

				foreach (var (pathId, referenceNodes) in referenceNodesToAssign)
				{
					if (rootNodeViewModel is not null && paths[pathId].TryNavigate(rootNodeViewModel, out var targetNode))
					{
						foreach (var referenceNode in referenceNodes)
						{
							referenceNode.Node = targetNode;
						}
					}
				}

				foreach (var message in messages)
				{
					var messageParameters = new MessageBoxParameters($"Error: {message.Message} Error occured here: '{string.Join("/", message.Path)}'", string.Empty, "Next");
					messageParameters.AddButton("Cancel");

					if (window.ShowMessage(messageParameters) == 1)
					{
						return stream;
					}
				}

				instance.HasChanges = false;
				instance.Root = rootNodeViewModel;
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
            var schemaType = await LoadSchema(window);
            if (schemaType is not null)
            {
                instance.Root = schemaType.Visit(new NodeViewModelGeneratorVisitor(instance, null, null));
                OpenFiles.Add(instance);
                SelectedFile = instance;
            }
		}

		[RelayCommand]
		async Task OpenFile(Window window)
		{
			var schemaType = await LoadSchema(window);

			if (schemaType is null)
			{
				return;
			}

			var openFileDialog = new OpenFileDialog()
			{
				Filter = "Json files (*.json)|*.json",
				InitialDirectory = @"C:\Database",
				Multiselect = true,
			};

			if (openFileDialog.ShowDialog() == true)
			{
				await using var streams = await OpenFilesAsync(window, schemaType, openFileDialog.FileNames.Select((fileName) => new FileInfo(fileName)));
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
            var tasks = new List<Task>();
			foreach (var file in OpenFiles)
            {
                tasks.Add(file.SaveAsync());
            }
            await Task.WhenAll(tasks);
		}

		[RelayCommand]
        void Exit(Window window)
        {
            window.Close();
        }
    }
}
