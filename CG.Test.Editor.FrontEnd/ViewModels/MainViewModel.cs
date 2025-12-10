using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public struct Vector3([Range(-100, 100)] float x, [Range(-100, 100)] float y, [Range(-100, 100)]  float z);
    public struct Transformation(Vector3 position, Vector3 rotation, Vector3 scale);

    public class RootNode(Transformation transformation, bool booleanValue, string stringValue, string[] names, bool[] flags)
    {
        
    }

    public class SchemaParsingMessageComparer : IEqualityComparer<SchemaParsingMessage>
    {
        public bool Equals(SchemaParsingMessage x, SchemaParsingMessage y)
        {
            return x.Message == y.Message;
        }

        public int GetHashCode([DisallowNull] SchemaParsingMessage obj)
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

        [RelayCommand]
        async Task NewFile(Window window)
        {
            ArgumentNullException.ThrowIfNull(window, nameof(window));
            var instance = new FileInstanceViewModel(this, window);

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Json Schema files (*.json) | *json"
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
        void Exit(Window window)
        {
            window.Close();
        }
    }
}
