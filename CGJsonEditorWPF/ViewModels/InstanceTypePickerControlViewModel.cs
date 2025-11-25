using CG.Test.Editor.Models;
using CG.Test.Editor.Models.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace CG.Test.Editor.ViewModels
{
    //public class EditorFolderTreeNode
    //{
    //    private readonly Func<EditorInstanceType, bool> _filter;

    //    public EditorFolderTreeNode(EditorFolder folder, Func<EditorInstanceType, bool> filter, string name = "")
    //    {
    //        _filter = filter;

    //        Name = name;

    //        Folder = folder;

    //        Children = [];

    //        foreach (var child in folder.Folders)
    //        {
    //            var childNode = new EditorFolderTreeNode(child, _filter, child.Name);
    //            if (childNode.Children.Count > 0 || childNode.Types.Any())
    //            {
    //                Children.Add(childNode);
    //            }
    //        }
    //    }

    //    public string Name { get; }

    //    public EditorFolder Folder { get; }

    //    public IEnumerable<EditorInstanceType> Types => Folder.Types.Where(_filter);

    //    public ObservableCollection<EditorFolderTreeNode> Children { get; }

    //    public bool TryFind(EditorFolder folder, [NotNullWhen(true)] out EditorFolderTreeNode? foundNode)
    //    {
    //        if (folder == Folder)
    //        {
    //            foundNode = this;
    //            return true;
    //        }

    //        foreach (var child in Children)
    //        {
    //            if (child.TryFind(folder, out foundNode))
    //            {
    //                return true;
    //            }
    //        }

    //        foundNode = null;
    //        return false;
    //    }
    //}


    public partial class InstanceTypePickerControlViewModel : ObservableObject
    {
        //private Func<EditorInstanceType, bool> _filter = (type) => true;

        //[ObservableProperty]
        //private EditorFolderTreeNode? _selectedFolder;

        //[ObservableProperty]
        //private EditorInstanceType? _selectedType;

        //public ObservableCollection<EditorFolderTreeNode> RootNodes { get; } = [];

        //public ObservableCollection<EditorInstanceType> Types { get; } = [];

        //public Func<EditorInstanceType, bool> Filter
        //{
        //    get => _filter;
        //    set
        //    {
        //        _filter = value;

        //        for (var i = 0; i < RootNodes.Count; i++)
        //        {
        //            RootNodes[i] = new EditorFolderTreeNode(RootNodes[i].Folder, value, RootNodes[i].Name);
        //            if (i == 0)
        //            {
        //                RefreshTypes(RootNodes[i]);
        //            }
        //        }
        //    }
        //}

        //private void RefreshTypes(EditorFolderTreeNode node)
        //{
        //    Types.Clear();
        //    foreach (var type in node.Types)
        //    {
        //        Types.Add(type);
        //    }
        //}

        //partial void OnSelectedFolderChanged(EditorFolderTreeNode? value)
        //{
        //    if (value is not null)
        //    {
        //        RefreshTypes(value);
        //    }
        //}

        //partial void OnSelectedTypeChanged(EditorInstanceType? value)
        //{
        //    if (value is not null && SelectedFolder?.Folder != value.Parent && RootNodes.Count > 0 && RootNodes[0].TryFind(value.Parent, out var folderNode))
        //    {
        //        SelectedFolder = folderNode;
        //    }
        //    SelectedTypeChanged?.Invoke(value);
        //}

        //[RelayCommand]
        //public void SelectFolder(EditorFolderTreeNode treeNode)
        //{
        //    SelectedFolder = treeNode;
        //}

        //[RelayCommand]
        //public void SelectType(EditorInstanceType type)
        //{
        //    SelectedType = type;
        //}

        //public event Action<EditorInstanceType?>? SelectedTypeChanged;
    }
}
