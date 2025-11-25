using CG.Test.Editor.Models.Nodes;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace CG.Test.Editor.ViewModels
{
    public partial class ObjectNodeViewModel : NodeViewModelBase
    {
        [ObservableProperty]
        private JsonObjectNode _objectNode;

        [ObservableProperty]
        private string? _name;

        public ObjectNodeViewModel(NodeEditorViewModel editor, JsonObjectNode objectNode) : base(editor, objectNode)
        {
            ObjectNode = objectNode;

            Nodes = new(objectNode.Values.Select((value, index) => new KeyValuePair<string, NodeViewModelBase>(_objectNode.Type.Fields[index].Name, FromNode(editor, value))));
        
            if (objectNode.TryGetValue("name", out var nameNode) && nameNode is JsonValueNode nameValue && nameValue.Value is string name)
            {
                Name = name;
            }
        }

        public ObservableCollection<KeyValuePair<string, NodeViewModelBase>> Nodes { get; }

        protected override void OnSave()
        {

        }
    }
}
