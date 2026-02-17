using CG.Test.Editor.FrontEnd.Models.Types;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using CG.Test.Editor.FrontEnd.Visitors;
using CommunityToolkit.Mvvm.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
    public class IsInsertEnabledConverter : MultiValueConverterBase<bool>
    {
        public bool Convert(int count, SchemaArrayType type)
        {
            return count + 1 <= type.MaximumItemCount;
        }
    }

	public class IsDeleteEnabledConverter : MultiValueConverterBase<bool>
	{
		public bool Convert(int elementCount, SchemaArrayType arrayType, int selectionCount)
		{
			return selectionCount > 0 && elementCount - selectionCount >= arrayType.MinimumItemCount;
		}
    }

	public class IsPasteEnabledConverter : MultiValueConverterBase<bool>
	{
		public bool Convert(int elementCount, SchemaArrayType arrayType, int clipboardCount)
		{
			return clipboardCount > 0 && elementCount + clipboardCount <= arrayType.MaximumItemCount;
		}
	}

	public partial class ArrayNodeViewModel : NodeViewModelBase
    {
        public ArrayNodeViewModel(NodeTree tree, NodeViewModelBase? parent, IEnumerable<NodeViewModelBase> nodes, SchemaArrayType type) : base(tree, parent)
        {
            Type = type;

            Elements = new(nodes);

			Indices = [];

            Elements.CollectionChanged += Elements_CollectionChanged;
		}

        public ObservableCollection<NodeViewModelBase> Elements { get; }

		public ObservableCollection<int> Indices { get; }

		public override SchemaArrayType Type { get; }

		private void Elements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Tree.Editor?.HasChanges = true;

            for (var i = Indices.Count; i < Elements.Count; i++)
            {
                Indices.Add(i);
            }

            for (var i = Elements.Count; i < Indices.Count; i++)
            {
                Indices.RemoveAt(Indices.Count - 1);
            }

            if (e.OldItems is not null)
            {
                foreach (var removedNode in e.OldItems.OfType<NodeViewModelBase>())
                {
                    removedNode.Release();
                }
            }
        }

        public override ArrayNodeViewModel Clone(NodeViewModelBase? parent)
        {
            var result = new ArrayNodeViewModel(Tree, parent, [], Type);
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
            return "Item [(Index not found!)]";
		}

        [RelayCommand]
        void Insert()
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                var itemDialog = new ItemCountDialog()
                {
                    MaximumItemCount = (uint)(Elements.Count - Type.MaximumItemCount)
                };

                if (itemDialog.ShowDialog() == true)
                {
                    var baseNode = Type.ElementType.Visit(new NodeViewModelGeneratorVisitor(Tree.Editor!.OwnerWindow, Tree, this, null));

                    if (baseNode is null)
                    {
                        return;
                    }

                    for (var i = 0; i < itemDialog.ItemCount; i++)
                    {
                        Elements.Add(baseNode.Clone(this));
                    }
                }
            }
            else
            {
                var newNode = Type.ElementType.Visit(new NodeViewModelGeneratorVisitor(Tree.Editor!.OwnerWindow, Tree, this, null));
                if (newNode is not null)
                {
                    Elements.Add(newNode);
                }
            }
        }

		[RelayCommand]
		void InsertAbove(int selectedIndex)
		{
			var newNode = Type.ElementType.Visit(new NodeViewModelGeneratorVisitor(Tree.Editor!.OwnerWindow, Tree, this, null));
            if (newNode is not null)
            {
                Elements.Insert(selectedIndex, newNode);
            }   
		}

		[RelayCommand]
		void InsertBelow(int selectedIndex)
		{
			var newNode = Type.ElementType.Visit(new NodeViewModelGeneratorVisitor(Tree.Editor!.OwnerWindow, Tree, this, null));
            if (newNode is not null)
            {
                Elements.Insert(selectedIndex + 1, newNode);
            }
		}

		[RelayCommand]
        void CutElements(IEnumerable selectedItems)
        {
			Tree.Editor!.ClipboardNodes = new(selectedItems.ClonedNodes(null));

            foreach (var node in Tree.Editor.ClipboardNodes)
            {
                Elements.Remove(node);
            }
        }

        [RelayCommand]
        void CopyElements(IEnumerable selectedItems)
        {
			Tree.Editor!.ClipboardNodes = new(selectedItems.ClonedNodes(null));
		}

        [RelayCommand]
        void PasteElements()
        {
			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
			{
				var itemDialog = new ItemCountDialog();
				if (itemDialog.ShowDialog() == true)
				{
					for (var i = 0; i < itemDialog.ItemCount; i++)
					{
						foreach (var node in Tree.Editor!.ClipboardNodes!.Where((node) => Type.ElementType.IsConvertibleFrom(node.Type)))
						{
							Elements.Add(node.Clone(this));
						}
					}
				}
			}
			else
            {
                foreach (var node in Tree.Editor!.ClipboardNodes!.Where((node) => Type.ElementType.IsConvertibleFrom(node.Type)))
                {
                    Elements.Add(node.Clone(this));
                }
            }
        }

        [RelayCommand]
        void PasteAboveElements(int selectedIndex)
        {
            var index = selectedIndex;
            foreach (var node in Tree.Editor!.ClipboardNodes!.Where((node) => Type.ElementType.IsConvertibleFrom(node.Type)))
            {
                Elements.Insert(index, node.Clone(this));
                ++index;
            }   
		}

		[RelayCommand]
		void PasteBelowElements(int selectedIndex)
		{
            var index = selectedIndex + 1;
            foreach (var node in Tree.Editor!.ClipboardNodes!.Where((node) => Type.ElementType.IsConvertibleFrom(node.Type)))
            {
                Elements.Insert(index, node.Clone(this));
                ++index;
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

        [RelayCommand]
        void SearchElements()
        {
            var dialog = new SearchArrayDialog()
            {
                NodeCollection = CollectionViewSource.GetDefaultView(new ObservableCollection<NodeViewModelBase>(Elements))
            };

            if (dialog.ShowDialog() == true)
            {
				dialog.SelectedNode.Visit(new NodeEditorVisitor(false));
			}
        }

        public override IReadOnlyList<NodeViewModelBase> Children => Elements;

        public override void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, ulong> referencedNodes)
        {
            writer.WriteStartArray();
            foreach (var element in Elements)
            {
                element.SerializeTo(writer, referencedNodes);
            }
            writer.WriteEndArray();
        }
    }
}
