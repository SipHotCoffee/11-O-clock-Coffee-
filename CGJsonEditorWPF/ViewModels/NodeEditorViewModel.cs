using CG.Test.Editor.Models.Nodes;
using CG.Test.Editor.Models.Types;
using CG.Test.Editor.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace CG.Test.Editor.ViewModels
{
    public partial class NamedNodeViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private NodeViewModelBase _nodeViewModel;

        public NamedNodeViewModel(string name, NodeViewModelBase node, NamedNodeViewModel? parent)
        {
            Name = name;
            NodeViewModel = node;

            Parent = parent;

            NodeViewModel.PropertyChanged += Node_PropertyChanged;
        }

        private void Node_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (NodeViewModel.Node is JsonObjectNode objectNode && objectNode.Type.TryGetField("name", out var field) && objectNode.Values[field.Index] is JsonValueNode nameValue && nameValue.Value is string name)
            {
                Name = name;
            }
        }

        public NamedNodeViewModel? Parent { get; }
    }

    public abstract partial class NodeViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private NodeEditorViewModel _editor;

        public NodeViewModelBase(NodeEditorViewModel editor, JsonNodeBase node)
        {
            Editor = editor;

            Node = node;
        }

        public static NodeViewModelBase FromNode(NodeEditorViewModel editor, JsonNodeBase node)
        {
            return node switch
            {
                 JsonArrayNode  arrayNode => new  ArrayNodeViewModel(editor, arrayNode),
                JsonObjectNode objectNode => new ObjectNodeViewModel(editor, objectNode),
                 JsonValueNode  valueNode => new  ValueNodeViewModel(editor, valueNode),

                _ => throw new NotImplementedException()
            };
        }

        public JsonNodeBase Node { get; }

        [RelayCommand]
        public void Save() => OnSave();

        protected virtual void OnSave() { }

        public virtual bool Navigate(string name) { return true; }
    }

    public partial class NodeEditorViewModel : ObservableObject
    {
        [ObservableProperty] 
        private int _pageHistoryIndex;

        [ObservableProperty]
        private Window? _ownerWindow;

        [ObservableProperty]
        private NodeViewModelBase? _root;

        [ObservableProperty]
        private NodeViewModelBase? _selectedNode;

        public ObservableCollection<NamedNodeViewModel> NavigationItems { get; } = [];

        public ObservableCollection<JsonNodeBase> ClipboardNodes { get; } = [];

        public List<NamedNodeViewModel> PageHistory { get; } = [];

        private void AddPage(NamedNodeViewModel node)
        {
            for (var i = 0; i < PageHistory.Count - PageHistoryIndex; i++)
            {
                PageHistory.RemoveAt(PageHistory.Count - 1);
            }

            PageHistory.Add(node);
            PageHistoryIndex = PageHistory.Count;
        }

        private void PopPage()
        {
            NavigationItems.RemoveAt(NavigationItems.Count - 1);

            var parent = NavigationItems[^1];
            ProcessNode(parent.Name, parent.NodeViewModel);
            AddPage(parent);
        }

        private void UpdateAddressBar(NamedNodeViewModel node)
        {
            NavigationItems.Clear();
            var item = node;
            for (; item.Parent is not null; item = item.Parent)
            {
                NavigationItems.Insert(0, item);
            }
            NavigationItems.Insert(0, item);
        }

        [RelayCommand]
        void GoUp()
        {
            PopPage();
        }

        [RelayCommand]
        void GoBack()
        {
            --PageHistoryIndex;

            var previous = PageHistory[PageHistoryIndex - 1];

            ProcessNode(previous.Name, previous.NodeViewModel);
            UpdateAddressBar(previous);
        }

        [RelayCommand]
        void GoForward()
        {
            ++PageHistoryIndex;

            var next = PageHistory[PageHistoryIndex - 1];

            ProcessNode(next.Name, next.NodeViewModel);
            UpdateAddressBar(next);
        }

        [RelayCommand]
        void OpenMultipleOperationDialog()
        {
            var rootNode = NavigationItems[0].NodeViewModel.Node;
            var viewModel = new MultipleOperationViewModel()
            {
                RootNode     = rootNode,
                OwnerWindow  = OwnerWindow,
                SelectedType = (JsonStructType)rootNode.Type,
            };

            var dialog = new MultipleOperationView()
            {
                Owner       = OwnerWindow,
                DataContext = viewModel,
            };

            viewModel.OwnerWindow = dialog;

            dialog.ShowDialog();
        }


        partial void OnRootChanged(NodeViewModelBase? oldValue, NodeViewModelBase? newValue)
        {
            if (newValue is null)
            {
                return;
            }

            PageHistory.Clear();
            NavigateNode("(Root)", newValue);
        }

        public void NavigateNode(string name, NodeViewModelBase node)
        {
            NamedNodeViewModel? parent = null;
            if (NavigationItems.Count > 0)
            {
                parent = NavigationItems[^1];
            }

            if (ProcessNode(name, node))
            {
                var newNode = new NamedNodeViewModel(name, SelectedNode!, parent);
                NavigationItems.Add(newNode);
                AddPage(newNode);
            }
        }

        public bool ProcessNode(string name, NodeViewModelBase node)
        {
            SelectedNode?.Save();
            if (node.Navigate(name))
            {
                SelectedNode = node;
                return true;
            }
            return false;
        }
    }
}
