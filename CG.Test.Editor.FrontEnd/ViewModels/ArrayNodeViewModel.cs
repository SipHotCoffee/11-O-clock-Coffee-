using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class ArrayNodeViewModel : NodeViewModelBase
    {
        public ArrayNodeViewModel(FileInstanceViewModel editor, NodeViewModelBase? parent, SchemaArrayType type) : base(editor, parent)
        {
            Type = type;

            Elements = [];

            Indices = [];

            Elements.CollectionChanged += Elements_CollectionChanged;
		}

		public ObservableCollection<NodeViewModelBase> Elements { get; }

		public ObservableCollection<int> Indices { get; }

		public override SchemaArrayType Type { get; }

		private void Elements_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (var i = Indices.Count; i < Elements.Count; i++)
            {
                Indices.Add(i);
            }

            for (var i = Elements.Count; i < Indices.Count; i++)
            {
                Indices.RemoveAt(Indices.Count - 1);
            }
        }

        public override ArrayNodeViewModel Clone(NodeViewModelBase? parent)
        {
            var result = new ArrayNodeViewModel(Editor, parent, Type);
            foreach (var element in Elements)
            {
                result.Elements.Add(element.Clone(result));
            }
            return result;
        }

        protected override string GetName(NodeViewModelBase item)
        {
            for (var i = 0; i < Elements.Count; i++)
			{
                var element = Elements[i];
                if (element == item)
				{
                    return $"Item: [{i}]";
				}
			}
            return base.GetName(item);
		}

        [RelayCommand]
        void Insert()
        {
            Elements.Add(Type.ElementType.Visit(new NodeViewModelGeneratorVisitor(Editor, this)));
        }

		[RelayCommand]
		void InsertAbove(int selectedIndex)
		{
			Elements.Insert(selectedIndex, Type.ElementType.Visit(new NodeViewModelGeneratorVisitor(Editor, this)));
		}

		[RelayCommand]
		void InsertBelow(int selectedIndex)
		{
			Elements.Insert(selectedIndex + 1, Type.ElementType.Visit(new NodeViewModelGeneratorVisitor(Editor, this)));
		}

		[RelayCommand]
        void CutElements(IEnumerable selectedItems)
        {
            Editor.ClipboardNodes.Clear();
            foreach (var selectedItem in selectedItems.OfType<NodeViewModelBase>())
            {
                Editor.ClipboardNodes.Add(selectedItem);
            }

            foreach (var node in Editor.ClipboardNodes)
            {
                Elements.Remove(node);
            }
        }

        [RelayCommand]
        void CopyElements(IEnumerable selectedItems)
        {
            Editor.ClipboardNodes.Clear();
			foreach (var selectedItem in selectedItems.OfType<NodeViewModelBase>().Select((node) => node.Clone(null)))
			{
				Editor.ClipboardNodes.Add(selectedItem);
			}
		}

        [RelayCommand]
        void PasteElements()
        {
            foreach (var node in Editor.ClipboardNodes.Where((node) => Type.ElementType.IsConvertibleFrom(node.Type)))
            {
                Elements.Add(node.Clone(this));
            }
        }

        [RelayCommand]
        void PasteAboveElements(int selectedIndex)
        {
            var index = selectedIndex;
			foreach (var node in Editor.ClipboardNodes.Where((node) => Type.ElementType.IsConvertibleFrom(node.Type)))
			{
				Elements.Insert(selectedIndex++, node.Clone(this));
			}
		}

		[RelayCommand]
		void PasteBelowElements(int selectedIndex)
		{
			var index = selectedIndex + 1;
			foreach (var node in Editor.ClipboardNodes.Where((node) => Type.ElementType.IsConvertibleFrom(node.Type)))
			{
				Elements.Insert(selectedIndex++, node.Clone(this));
			}
		}

        [RelayCommand]
        void DeleteElements(IEnumerable selectedItems)
        {
            var nodesToRemove = selectedItems.OfType<NodeViewModelBase>().ToList();
            foreach (var nodeToRemove in nodesToRemove)
            {
                Elements.Remove(nodeToRemove);
            }
		}

        [RelayCommand]
        void MoveElementsUp(ListBox listBox)
        {
            var currentIndex = listBox.SelectedIndex;
			var newIndex = currentIndex - 1;
            if (newIndex < 0)
            {
                newIndex = Elements.Count - 1;
            }

            (Elements[currentIndex], Elements[newIndex]) = (Elements[newIndex], Elements[currentIndex]);
            listBox.SelectedIndex = newIndex;
        }

        [RelayCommand]
		void MoveElementsDown(ListBox listBox)
		{
			var currentIndex = listBox.SelectedIndex;
			var newIndex = currentIndex + 1;
			if (newIndex >= Elements.Count)
			{
				newIndex = 0;
			}

			(Elements[currentIndex], Elements[newIndex]) = (Elements[newIndex], Elements[currentIndex]);
			listBox.SelectedIndex = newIndex;
		}
	}
}
