using CommunityToolkit.Mvvm.ComponentModel;

namespace CG.Test.Editor.ViewModels
{
    //public partial class EditorNamedNode : ObservableObject
    //{
    //    [ObservableProperty]
    //    private string _name;

    //    public EditorNamedNode(string name, JsonNodeBase node, EditorNamedNode? parent)
    //    {
    //        Name   = name;
    //        Node   = node;
    //        Parent = parent;

    //        Node.PropertyChanged += Node_PropertyChanged;
    //    }

    //    private void Node_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    //    {
    //        if (Node is JsonObjectNode objectNode && objectNode.Nodes.TryGetValue("name", out var nameNode) && nameNode is JsonValueNode nameValue && nameValue.Value is string name)
    //        {
    //            Name = name;
    //        }
    //    }

    //    public JsonNodeBase Node { get; }

    //    public EditorNamedNode? Parent { get; }
    //}

    public partial class ObjectEditorViewModel : ObservableObject
    {
        //[ObservableProperty]
        //private Window? _ownerWindow;

        //[ObservableProperty]
        //private ObservableCollection<EditorNamedNode> _pageHistory = [];

        //[ObservableProperty]
        //private int _pageHistoryIndex;

        //public ObservableCollection<EditorNamedNode> AddressItems { get; } = [];

        //[ObservableProperty]
        //private JsonNodeBase? _root;
        
        //[ObservableProperty]
        //private UserControl? _currentView;

        //[ObservableProperty]
        //private JsonStructType? _first;

        //private const string ROOT_NAME = "(Root)";
        
        //public ObservableCollection<JsonNodeBase> ClipboardNodes { get; } = [];

        //partial void OnRootChanged(JsonNodeBase? oldValue, JsonNodeBase? newValue)
        //{
        //    if (newValue is null)
        //    {
        //        return;
        //    }

        //    NavigateNode(ROOT_NAME, newValue);
        //}

        //[RelayCommand]
        //void GoUp()
        //{
        //    PopPage();
        //}

        //[RelayCommand]
        //void GoBack()
        //{
        //    --PageHistoryIndex;

        //    var previous = PageHistory[PageHistoryIndex - 1];

        //    ProcessNode(previous.Name, previous.Node);
        //    UpdateAddressBar(previous);
        //}

        //[RelayCommand]
        //void GoForward()
        //{
        //    ++PageHistoryIndex;

        //    var next = PageHistory[PageHistoryIndex - 1];

        //    ProcessNode(next.Name, next.Node);
        //    UpdateAddressBar(next);
        //}

        //[RelayCommand]
        //void GoToNode(EditorNamedNode node)
        //{
        //    UpdateAddressBar(node);
        //    ProcessNode(node.Name, node.Node);
        //}

        //[RelayCommand]
        //void OpenMultipleOperationDialog()
        //{
        //    var viewModel = new MultipleOperationViewModel()
        //    {
        //        RootNode     = AddressItems[^1].Node,
        //        OwnerWindow  = OwnerWindow,
        //        SelectedType = First,
        //    };

        //    var dialog = new MultipleOperationView()
        //    {
        //        Owner = OwnerWindow,
        //        DataContext = viewModel,
        //    };

        //    viewModel.OwnerWindow = dialog;

        //    dialog.ShowDialog();
        //}

        //private void UpdateAddressBar(EditorNamedNode node)
        //{
        //    AddressItems.Clear();
        //    var item = node;
        //    for (; item.Parent is not null; item = item.Parent)
        //    {
        //        AddressItems.Insert(0, item);
        //    }
        //    AddressItems.Insert(0, item);
        //}

        //public void NavigateNode(string name, JsonNodeBase node)
        //{
        //    if (node is JsonArrayNode || node is JsonObjectNode)
        //    {
        //        EditorNamedNode? parent = null;
        //        if (AddressItems.Count > 0)
        //        {
        //            parent = AddressItems[^1];
        //        }

        //        var newNode = new EditorNamedNode(name, node, parent);
        //        AddressItems.Add(newNode);
        //        AddPage(newNode);
        //    }

        //    ProcessNode(name, node);
        //}

        //public void ProcessNode(string name, JsonNodeBase node)
        //{
        //    switch (node)
        //    {
        //        case JsonObjectNode objectNode: ProcessObject(objectNode);     break;
        //        case JsonArrayNode   arrayNode: ProcessArray(arrayNode);       break;
        //        case JsonValueNode   valueNode: ProcessValue(name, valueNode); break;
        //    }

        //    First = AddressItems[^1].Node.GetFirstStructure();
        //}

        //private void ProcessObject(JsonObjectNode objectNode)
        //{
        //    var viewModel = new ObjectViewModel()
        //    {
        //        Editor = this,
        //        ObjectNode = objectNode
        //    };

        //    CurrentView = new ObjectView() 
        //    { 
        //        DataContext = viewModel
        //    };
        //}

        //private void ProcessArray(JsonArrayNode arrayNode)
        //{
        //    var viewModel = new ArrayViewModel()
        //    {
        //        Editor = this,
        //        ArrayNode = arrayNode
        //    };

        //    CurrentView = new ArrayView()
        //    {
        //        DataContext = viewModel
        //    };
        //}

        //private void ProcessValue(string name, JsonValueNode value)
        //{
        //    value.Process(OwnerWindow!, name, (selectedObjectNode) =>
        //    {
        //        var nodePath = new List<JsonNodeBase>();
        //        for (JsonNodeBase? node = selectedObjectNode; node is not null; node = node.Parent)
        //        {
        //            nodePath.Insert(0, node);
        //        }

        //        AddressItems.Clear();
        //        EditorNamedNode? parent = null;
        //        foreach (var node in nodePath)
        //        {
        //            if (node is not JsonObjectNode childObjectNode ||
        //                !childObjectNode.Nodes.TryGetValue("name", out var foundNode) ||
        //                foundNode is not JsonValueNode valueNode ||
        //                valueNode.Value is not string childName)
        //            {
        //                if (parent?.Node is JsonArrayNode parentArrayNode)
        //                {
        //                    childName = $"Element: [{parentArrayNode.Elements.IndexOf(node)}]";
        //                }
        //                else if (parent?.Node is JsonObjectNode parentObjectNode && parentObjectNode.Nodes.TryGetKey(node, out var key))
        //                {
        //                    childName = key;
        //                }
        //                else
        //                {
        //                    childName = ROOT_NAME;
        //                }
        //            }

        //            var current = new EditorNamedNode(childName, node, parent);
        //            AddressItems.Add(current);
        //            parent = current;
        //        }

        //        ++PageHistoryIndex;
        //        AddPage(AddressItems[^1]);
        //        ProcessObject(selectedObjectNode);
        //    });
        //}

        //private void AddPage(EditorNamedNode node)
        //{
        //    for (var i = 0; i < PageHistory.Count - PageHistoryIndex; i++)
        //    {
        //        PageHistory.RemoveAt(PageHistory.Count - 1);
        //    }

        //    PageHistory.Add(node);
        //    PageHistoryIndex = PageHistory.Count;
        //}

        //private void PopPage()
        //{
        //    AddressItems.RemoveAt(AddressItems.Count - 1);

        //    var parent = AddressItems[^1];
        //    ProcessNode(parent.Name, parent.Node);
        //    AddPage(parent);
        //}
    }
}
