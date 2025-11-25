using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CG.Test.Editor.Models.Nodes;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace CG.Test.Editor.ViewModels
{
    public class EditorNodeTreeNode
    {
        private readonly Func<JsonNodeBase, bool> _filter;

        public EditorNodeTreeNode(JsonNodeBase node, Func<JsonNodeBase, bool> filter, string name = "(Root)")
        {
            _filter = filter;

            Name = name;
            Node = node;

            Children = [];

            foreach (var child in node.Children)
            {
                var childNode = new EditorNodeTreeNode(child, _filter, child.ToString());
                if (_filter(child) || childNode.Children.Count > 0)
                {
                    Children.Add(childNode);
                }
            } 
        }

        public string Name { get; }

        public JsonNodeBase Node { get; }

        public ObservableCollection<EditorNodeTreeNode> Children { get; }
    }

    public partial class ReferenceDialogViewModel : ObservableObject
    {
        private Func<JsonNodeBase, bool> _filter = (type) => true;

        [ObservableProperty]
        private bool _hasReference;

        [ObservableProperty]
        private JsonNodeBase? _selectedNode;

        [ObservableProperty]
        private bool _hasSelection;

        [ObservableProperty]
        private bool _hasTargetLocation;

        [ObservableProperty]
        private Visibility _objectViewVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility _goToRefLocationVisibility = Visibility.Visible;

        [ObservableProperty]
        private JsonObjectNode? _selectedObject;

        [ObservableProperty]
        private string _searchText = string.Empty;

        public ObservableCollection<EditorNodeTreeNode> RootNodes { get; } = [];

        public ReferenceDialogViewModel()
        {
            HasSelection = true;
        }

        private void UpdateHasSelection()
        {
            HasSelection = !HasReference || (SelectedNode is not null && _filter(SelectedNode));

            if (HasReference && SelectedNode is JsonObjectNode objectNode)
            {
                ObjectViewVisibility = Visibility.Visible;
                SelectedObject = objectNode;
            }
            else
            {
                ObjectViewVisibility = Visibility.Collapsed;
                SelectedObject = null;
            }
        }

        partial void OnHasReferenceChanged(bool oldValue, bool newValue)
        {
            UpdateHasSelection();
        }

        partial void OnSelectedNodeChanged(JsonNodeBase? oldValue, JsonNodeBase? newValue)
        {
            UpdateHasSelection();
        }

        public Func<JsonNodeBase, bool> Filter
        {
            get => _filter;
            set
            {
                _filter = value;

                for (var i = 0; i < RootNodes.Count; i++)
                {
                    RootNodes[i] = new EditorNodeTreeNode(RootNodes[i].Node, value, RootNodes[i].Name);
                }
            }
        }

        [RelayCommand]
        void Search()
        {
            if (SearchText == string.Empty)
            {
                for (var i = 0; i < RootNodes.Count; i++)
                {
                    RootNodes[i] = new EditorNodeTreeNode(RootNodes[i].Node, Filter, RootNodes[i].Name);
                }
            }
            else
            {
                for (var i = 0; i < RootNodes.Count; i++)
                {
                    RootNodes[i] = new EditorNodeTreeNode(RootNodes[i].Node, (node) =>
                    {
                        if (Filter(node))
                        {
                            if (node is JsonObjectNode objectNode && objectNode.TryGetValue("name", out var childNameNode) && childNameNode is JsonValueNode childNameValue && childNameValue.Value is string childName)
                            {
                                return childName.Contains(SearchText);
                            }
                        }

                        return false;
                    },
                    RootNodes[i].Name);
                }
            }
        }

        [RelayCommand]
        void Ok(Window window)
        {
            window.DialogResult = true;
            HasTargetLocation   = false;
            window.Close();
        }

        [RelayCommand]
        void GoToRefLocation(Window window)
        {
            window.DialogResult = false;
            HasTargetLocation   = HasReference;
            window.Close();
        }
    }
}
