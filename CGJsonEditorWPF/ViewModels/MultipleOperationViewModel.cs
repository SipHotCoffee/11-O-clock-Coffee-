using CG.Test.Editor.Models.Nodes;
using CG.Test.Editor.Models.Types;
using CG.Test.Editor.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace CG.Test.Editor.ViewModels
{
    public class FieldNode
    {
        public FieldNode(JsonField field)
        {
            Children = [];

            Field = field;

            if (field.Type is JsonStructType structType)
            {
                foreach (var childField in structType.Fields)
                {
                    Children.Add(new FieldNode(childField));
                }
            }
        }

        public JsonField Field { get; }

        public List<FieldNode> Children { get; }
    }

    public partial class MultipleOperationViewModel : ObservableObject
    {
        [ObservableProperty]
        private Window? _ownerWindow;

        [ObservableProperty]
        private NodeEditorViewModel? _editor;

        [ObservableProperty]
        private JsonStructType? _selectedType;

        [ObservableProperty]
        private JsonNodeBase? _rootNode;

        [ObservableProperty]
        private JsonValueNode _oldValue = new(0);

        [ObservableProperty]
        private JsonValueNode _newValue = new(0);

        [ObservableProperty]
        private bool _hasOldValue = false;

        [ObservableProperty]
        private Visibility _replaceControlsVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private ObservableCollection<FieldNode> _fieldNodes = [];

        [ObservableProperty]
        private FieldNode? _selectedFieldNode;

        partial void OnSelectedTypeChanged(JsonStructType? oldValue, JsonStructType? newValue)
        {
            if (newValue is not null)
            {
                FieldNodes.Clear();
                foreach (var field in newValue.Fields)
                {
                    if (field.Type is not JsonArrayType)
                    {
                        FieldNodes.Add(new FieldNode(field));
                    }
                }
            }
        }

        [RelayCommand]
        void OpenTypePickerDialog()
        {
            var structPickerViewModel = new StructTypePickerViewModel()
            {
                SelectedType = SelectedType
            };

            var structPickerDialogViewModel = new StructTypePickerView()
            {
                DataContext = structPickerViewModel,
                Owner       = OwnerWindow,
            };

            if (structPickerDialogViewModel.ShowDialog() == true)
            {
                SelectedType = structPickerViewModel.SelectedType;
            }
        }

        [RelayCommand]
        void Select(FieldNode? node)
        {
            if (node is null)
            {
                ReplaceControlsVisibility = Visibility.Collapsed;
                return;
            }
            
            if (node.Field.Type is JsonValueType valueType)
            {
                OldValue = valueType.Create();
                NewValue = valueType.Create();
                ReplaceControlsVisibility = Visibility.Visible;
                SelectedFieldNode = node;
            }
        }

        [RelayCommand]
        void ProcessOldValue()
        {
            Editor!.ProcessNode(string.Empty, new ValueNodeViewModel(Editor, OldValue));
        }

        [RelayCommand]
        void ProcessNewValue()
        {
            Editor!.ProcessNode(string.Empty, new ValueNodeViewModel(Editor, NewValue));
        }

        [RelayCommand]
        void Replace()
        {
            var changeCount = UpdateChildren(RootNode!, HasOldValue ? OldValue : null, NewValue);
            MessageBox.ShowMessage(OwnerWindow!, $"Replaced {changeCount} occurrences.");
        }

        private int UpdateChildren(JsonNodeBase node, JsonValueNode? oldValue, JsonValueNode newValue)
        {
            if (node is JsonObjectNode objectNode &&
                objectNode.Type is JsonStructType structure &&
                SelectedType!.IsConvertibleFrom(structure) &&
                objectNode.Values[SelectedFieldNode!.Field.Index] is JsonValueNode valueNode &&
                (oldValue is null || Equals(valueNode.Value, oldValue.Value)))
            {
                valueNode.Value = newValue.Value;
                return 1;
            }
            else
            {
                var result = 0;
                foreach (var child in node.Children)
                {
                    result += UpdateChildren(child, oldValue, newValue);
                }
                return result;
            }
        }
    }
}
