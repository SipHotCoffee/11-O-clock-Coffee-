using CG.Test.Editor.Models.Nodes;
using CG.Test.Editor.Models.Types;
using CG.Test.Editor.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace CG.Test.Editor.ViewModels
{
    public partial class ArrayNodeViewModel : NodeViewModelBase
    {
        [ObservableProperty]
        private JsonArrayNode _arrayNode;

        [ObservableProperty]
        private int _count;

        public ObservableCollection<NodeViewModelBase> Elements { get; }

        public ObservableCollection<int> Indices { get; }

        public ArrayNodeViewModel(NodeEditorViewModel editor, JsonArrayNode arrayNode) : base(editor, arrayNode)
        {
            ArrayNode = arrayNode;

            Count = arrayNode.Elements.Count;

            Elements = new(arrayNode.Elements.Select((node) => FromNode(editor, node)));

            Indices = new(arrayNode.Elements.Select((node, index) => index));

            Elements.CollectionChanged += Elements_CollectionChanged;
        }

        private void Elements_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (var i = Indices.Count; i < Elements.Count; i++)
            {
                Indices.Add(i);
            }

            for (var i = Indices.Count; i > Elements.Count; i--)
            {
                Indices.RemoveAt(Indices.Count - 1);
            }

            Count = Elements.Count;
        }

        [RelayCommand]
        void AddElement(Window owner)
        {
            var elementType = ArrayNode.Type.ElementType;
            if (elementType is JsonStructType structType && structType.DerivedTypes.Count > 0)
            {
                var structTypePickerViewModel = new StructTypePickerViewModel()
                {
                    BaseType = structType,
                };

                var structTypePickerDialog = new StructTypePickerView()
                {
                    Owner = owner,
                    DataContext = structTypePickerViewModel
                };

                if (structTypePickerDialog.ShowDialog() == true && structTypePickerViewModel.SelectedType is not null)
                {
                    elementType = structTypePickerViewModel.SelectedType;
                }
                else
                {
                    return;
                }
            }
            Elements.Add(FromNode(Editor, elementType.Create()));
        }

        [RelayCommand]
        void CutElements(IEnumerable selectedItems)
        {
            Editor.ClipboardNodes.Clear();
            var selectedElements = selectedItems.OfType<NodeViewModelBase>().ToList();
            foreach (var element in selectedElements)
            {
                Elements.Remove(element);
                Editor.ClipboardNodes.Add(element.Node);
            }
        }

        [RelayCommand]
        void CopyElements(IEnumerable selectedItems)
        {
            Editor.ClipboardNodes.Clear();
            var selectedElements = selectedItems.OfType<JsonNodeBase>().ToList();
            foreach (var element in selectedElements)
            {
                Editor.ClipboardNodes.Add(element.Clone());
            }
        }

        [RelayCommand]
        void PasteElements()
        {
            var clipboardNodes = Editor.ClipboardNodes.Where((node) => ArrayNode.Type.ElementType.IsConvertibleFrom(node.Type)).ToList();

            foreach (var node in clipboardNodes)
            {
                Elements.Add(FromNode(Editor, node));
                Editor.ClipboardNodes.Remove(node);
                Editor.ClipboardNodes.Add(node.Clone());
            }
        }

        [RelayCommand]
        void PasteElementsBelow(int selectedIndex)
        {
            var clipboardNodes = Editor.ClipboardNodes.Where((node) => ArrayNode.Type.ElementType.IsConvertibleFrom(node.Type)).ToList();

            for (var i = 0; i < clipboardNodes.Count; i++)
            {
                var node = clipboardNodes[i];
                Elements.Insert(selectedIndex + 1 + i, FromNode(Editor, node));
                Editor.ClipboardNodes.Remove(node);
                Editor.ClipboardNodes.Add(node.Clone());
            }
        }

        [RelayCommand]
        void PasteElementsAbove(int selectedIndex)
        {
            if (selectedIndex == -1)
            {
                PasteElements();
                return;
            }

            var clipboardNodes = Editor.ClipboardNodes.Where((node) => ArrayNode.Type.ElementType.IsConvertibleFrom(node.Type)).ToList();

            for (var i = 0; i < clipboardNodes.Count; i++)
            {
                var node = clipboardNodes[i];
                Elements.Insert(selectedIndex + i, FromNode(Editor, node));
                Editor.ClipboardNodes.Remove(node);
                Editor.ClipboardNodes.Add(node.Clone());
            }
        }

        [RelayCommand]
        void DeleteElements(IEnumerable selectedItems)
        {
            var selectedElements = selectedItems.OfType<NodeViewModelBase>().ToList();
            foreach (var element in selectedElements)
            {
                Elements.Remove(element);
            }
        }

        [RelayCommand]
        void MoveElementUp(int index)
        {
            Elements.Move(index, index - 1);
        }

        [RelayCommand]
        void MoveElementDown(int index)
        {
            Elements.Move(index, index + 1);
        }

        [RelayCommand]
        void SearchElements()
        {
            Save();
            var viewModel = new ArrayElementSearchViewModel()
            {
                Elements = CollectionViewSource.GetDefaultView(new ObservableCollection<NodeViewModelBase>(Elements))
            };

            var dialog = new ArrayElementSearchDialogView()
            {
                Owner       = Editor.OwnerWindow,
                DataContext = viewModel,
            };

            if (dialog.ShowDialog() == true && viewModel.SelectedNode is not null)
            {
                Navigate(viewModel.SelectedNode);
            }
        }

        public void Navigate(JsonNodeBase node)
        {
            var index = ArrayNode.Elements.IndexOf(node);
            var name = $"Element: [{index}]";
            if (node is JsonObjectNode objectNode && objectNode.TryGetValue("name", out var foundNode) && foundNode is JsonValueNode valueNode && valueNode.Value is string foundName)
            {
                name = foundName;
            }

            Editor.NavigateNode(name, Elements[index]);
        }

        protected override void OnSave()
        {
            ArrayNode.Elements.Clear();
            ArrayNode.Elements.AddRange(Elements.Select((node) => node.Node));
        }
    }
}
