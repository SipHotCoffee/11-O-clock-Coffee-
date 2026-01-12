using CG.Test.Editor.FrontEnd.Models;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using CG.Test.Editor.FrontEnd.Visitors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

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
        [ObservableProperty]
        private FileInstanceViewModel? _selectedFile;

        public ObservableCollection<FileInstanceViewModel> OpenFiles { get; } = [];

        private async Task SaveFileAsync(Stream stream)
        {
			await using var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();
            {
                writer.WriteStartArray("referencePaths");
                {
                    foreach (var (node, id) in SelectedFile!.CachedPaths)
                    {
                        writer.WriteStartObject();
                        {
                            writer.WriteNumber("id", id);
                            writer.WriteStartArray("path");
                            {
                                foreach (var element in node.Address)
                                {
                                    element.SerializeTo(writer);
                                }
                            }
                            writer.WriteEndObject();
                        }
                        writer.WriteEndObject();
                    }
                }
                writer.WriteEndArray();

                writer.WritePropertyName("content");
                SelectedFile!.Root!.SerializeTo(writer);
            }
            writer.WriteEndObject();

			await writer.FlushAsync();
            SelectedFile.Root.HasChanges = false;
		}

		private async Task SaveAsFileAsync()
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Filter = "Json Schema files (*.json)|*.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await using var stream = saveFileDialog.OpenFile();
                await SaveFileAsync(stream);
			}
        }

        private static async Task<SchemaTypeBase?> LoadSchema(Window window)
        {
			ArgumentNullException.ThrowIfNull(window, nameof(window));

            var recentSchemasDialog = new RecentSchemasDialog();

			if (recentSchemasDialog.ShowDialog() == true)
			{
				try
				{
					await using var stream = File.OpenRead(recentSchemasDialog.SelectedSchema);
					var node = await JsonNode.ParseAsync(stream);

					var messages = new HashSet<SchemaParsingMessage>(10, new SchemaParsingMessageComparer());
					var logger = new CollectionLogger<SchemaParsingMessage>(messages);

					if (node is not null && node.TryParseSchemaType(logger, out var type))
					{
                        return type;
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

		[RelayCommand]
        async Task NewFile(Window window)
        {
			var instance = new FileInstanceViewModel(this, null, window);
            var schemaType = await LoadSchema(window);
            if (schemaType is not null)
            {
                instance.Root = schemaType.Visit(new NodeViewModelGeneratorVisitor(instance, null));
                OpenFiles.Add(instance);
                SelectedFile = instance;
            }
		}

		[RelayCommand]
		async Task OpenFile(Window window)
        {
			var instance = new FileInstanceViewModel(this, null, window);
			var schemaType = await LoadSchema(window);

			var openFileDialog = new OpenFileDialog()
			{
				Filter = "Json files (*.json)|*.json"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				await using var stream = openFileDialog.OpenFile();
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
                        if (window.ShowMessage($"Error: {message.Message} Error occured here: '{string.Join("/", message.Path)}'", string.Empty, 0, "Next", [ "Cancel" ]) == 1)
                        {
                            return;
                        }
                    }

					instance.Root = rootNodeViewModel;
					OpenFiles.Add(instance);
					SelectedFile = instance;

                }
                catch (Exception exception)
                {
                    window.ShowMessage(exception.ToString());
                }
            }
        }

        [RelayCommand]
        async Task SaveFile()
        {
            if (SelectedFile!.File?.Exists != true)
            {
                await SaveAsFileAsync();
            }
            else if (SelectedFile!.Root!.HasChanges)
            {
                await using var stream = SelectedFile.File.OpenWrite();
                await SaveFileAsync(stream);
            }
        }

        [RelayCommand]
        async Task SaveFileAs()
        {
			await SaveAsFileAsync();
		}

        [RelayCommand]
        void Exit(Window window)
        {
            window.Close();
        }
    }
}
