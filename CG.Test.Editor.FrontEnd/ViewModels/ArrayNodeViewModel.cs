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

        [ObservableProperty]
        private bool _hasSelection;

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
        void CutElements(IEnumerable selectedItems)
        {
            Editor.ClipboardNodes.Clear();
            Editor.ClipboardNodes.AddRange(selectedItems.OfType<NodeViewModelBase>());

            foreach (var node in Editor.ClipboardNodes)
            {
                Elements.Remove(node);
            }
        }

        [RelayCommand]
        void CopyElements(IEnumerable selectedItems)
        {
            Editor.ClipboardNodes.Clear();
            Editor.ClipboardNodes.AddRange(selectedItems.OfType<NodeViewModelBase>().Select((node) => node.Clone(null)));
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
            var startTargetIndex = listBox.SelectedIndex;

			var nodesToMove = listBox.SelectedItems.OfType<NodeViewModelBase>().ToList();
			foreach (var nodeToRemove in nodesToMove)
			{
				Elements.Remove(nodeToRemove);
			}

			foreach (var nodeToInsert in nodesToMove)
			{
				Elements.Insert(startTargetIndex, nodeToInsert);
			}
		}

		[RelayCommand]
		void MoveElementsDown(ListBox listBox)
		{
			var startTargetIndex = listBox.SelectedIndex + 1;

			var nodesToMove = listBox.SelectedItems.OfType<NodeViewModelBase>().ToList();
			foreach (var nodeToRemove in nodesToMove)
			{
				Elements.Remove(nodeToRemove);
			}

			foreach (var nodeToInsert in nodesToMove)
			{
				Elements.Insert(startTargetIndex, nodeToInsert);
			}
		}
	}
}
