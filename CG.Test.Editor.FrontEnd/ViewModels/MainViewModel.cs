using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
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
			SelectedFile!.Root!.SerializeTo(writer);
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

		[RelayCommand]
        async Task NewFile(Window window)
        {
            ArgumentNullException.ThrowIfNull(window, nameof(window));
            var instance = new FileInstanceViewModel(this, null, window);

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Json Schema files (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    await using var stream = openFileDialog.OpenFile();
                    var node = await JsonNode.ParseAsync(stream);

                    var messages = new HashSet<SchemaParsingMessage>(10, new SchemaParsingMessageComparer());
                    var logger = new CollectionLogger<SchemaParsingMessage>(messages);

                    if (node is not null && node.TryParseSchemaType(logger, out var type))
                    {
                        instance.Root = type.Visit(new NodeViewModelGeneratorVisitor(instance, null));
                        OpenFiles.Add(instance);
                        SelectedFile = instance;
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
		}

		[RelayCommand]
		void OpenFile(Window window)
        {

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
