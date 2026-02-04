using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml.Linq;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class FileInstanceViewModel : ObservableObject
    {
        private static int _lastId = 1;

        private readonly List<NodeViewModelBase> _history;

		[ObservableProperty]
		private MainViewModel _mainViewModel;

		[ObservableProperty]
        private int _historyIndex;

        [ObservableProperty]
        private bool _isBackButtonEnabled;

        [ObservableProperty]
        private bool _isForwardButtonEnabled;

        [ObservableProperty]
        private string _name;

		[ObservableProperty]
		private NodeViewModelBase? _root;

		[ObservableProperty]
		private NodeViewModelBase? _current;

        [ObservableProperty]
        private bool _hasClipboardNodes;

		[ObservableProperty]
		private ObservableCollection<NodeViewModelBase>? _clipboardNodes;

		public FileInstanceViewModel(MainViewModel mainViewModel, FileInfo? file, Window ownerWindow)
        {
			_history = [];
            
            MainViewModel = mainViewModel;

			HistoryIndex = 0;

            File = file;

			OwnerWindow = ownerWindow;

            Name = file?.Name ?? $"Untitled {_lastId++}";

			AddressItems = [];

            CachedPaths = new();

            HasClipboardNodes = false;
		}

        public FileInfo? File { get; set; }

		public Window OwnerWindow { get; }

		public ObservableCollection<NodeViewModelBase> AddressItems { get; }

        public CachedNodeReferenceCollection CachedPaths { get; }

        partial void OnClipboardNodesChanged(ObservableCollection<NodeViewModelBase>? oldValue, ObservableCollection<NodeViewModelBase>? newValue)
        {
            HasClipboardNodes = newValue is not null;
        }

		partial void OnRootChanged(NodeViewModelBase? oldValue, NodeViewModelBase? newValue)
        {
			Current = newValue;
        }

        partial void OnHistoryIndexChanged(int oldValue, int newValue)
        {
               IsBackButtonEnabled = newValue > 0;
            IsForwardButtonEnabled = newValue < _history.Count - 1;
        }

        partial void OnCurrentChanged(NodeViewModelBase? oldValue, NodeViewModelBase? newValue)
        {
            if (newValue is null)
            {
                return;
            }

            AddressItems.Clear();
            for (var current = newValue; current is not null; current = current.Parent)
            {
                AddressItems.Insert(0, current);
            }
		}

		private async Task SaveFileAsync(Stream stream)
		{
			await using var writer = new Utf8JsonWriter(stream);
			writer.WriteStartObject();
			{
				writer.WriteStartArray("referencePaths");
				{
					foreach (var (node, id) in CachedPaths)
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
				Root!.SerializeTo(writer);
			}
			writer.WriteEndObject();

			await writer.FlushAsync();
			Root.HasChanges = false;
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
				File = new FileInfo(saveFileDialog.FileName);
			}
		}

		public async Task Save()
        {
			if (File?.Exists != true)
			{
				await SaveAsFileAsync();
			}
			else if (Root!.HasChanges)
			{
				await using var stream = File.OpenWrite();
				await SaveFileAsync(stream);
			}
		}

        public async Task SaveAs()
        {
			await SaveAsFileAsync();
		}

        private void AddPage()
        {
            if (Current is null)
            {
                return;
            }

            var nextIndex = HistoryIndex + 1;
            if (nextIndex < _history.Count)
            {
                _history.RemoveRange(nextIndex, _history.Count - nextIndex);
            }
            _history.Add(Current);
			HistoryIndex = _history.Count - 1;
		}

        [RelayCommand]
        void MoveUp()
        {
			Current = Current?.Parent ?? throw new NullReferenceException();
			AddPage();
		}

        [RelayCommand]
        void MoveBack()
        {
            Current = _history[--HistoryIndex];
        }

        [RelayCommand]
        void MoveForward()
        {
			Current = _history[++HistoryIndex];
		}

		public async Task<bool> CloseAsync()
		{
			if (Root?.HasChanges == true)
			{
				var messageBoxParameters = new MessageBoxParameters($"Save changes to '{Name}'?", "CG Json Editor", "Yes");
				messageBoxParameters.AddButton("No");
				messageBoxParameters.AddButton("Cancel", true);

				switch (OwnerWindow.ShowMessage(messageBoxParameters))
				{
					case 0:
						await Save();
						break;
					case 2:
						return false;
				}
			}

			MainViewModel.OpenFiles.Remove(this);
			return true;
		}

		[RelayCommand]
		async Task Close()
		{
			await CloseAsync();
		}

        [RelayCommand]
        void Repair()
        {
            var availableTypes = Current!.AllChildren.Select((node) => node.Type).OfType<LinkedSchemaObjectType>().ToHashSet();

            var repairDialog = new RepairDialog(this, Current)
            {
                AvailableTypes = new(availableTypes)
            };

            repairDialog.ShowDialog();
        }

		public void Navigate(NodeViewModelBase target)
        {
			Current = target;
			AddPage();
		}
	}
}
