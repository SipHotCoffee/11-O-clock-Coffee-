using CG.Test.Editor.FrontEnd.Models.Types;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using CG.Test.Editor.FrontEnd.Visitors;
using CommunityToolkit.Mvvm.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels.Nodes
{
	public class NodeEntryViewModel
	{
		public required string PropertyName { get; init; }

		public required NodeViewModelBase Node { get; init; }

		public required bool IsAdditional { get; init; } 

		public void Deconstruct(out string propertyName, out NodeViewModelBase node)
		{
			propertyName = PropertyName;
			node = Node;
		}
	}

	public partial class ObjectNodeViewModel : NodeViewModelBase
	{
		private readonly Dictionary<string, int> _nodeMap;

		public ObjectNodeViewModel(NodeTree tree, NodeViewModelBase? parent, SchemaObjectType type) : base(tree, parent)
		{
			_nodeMap = [];

			Nodes = [];
			Nodes.CollectionChanged += Nodes_CollectionChanged;

			Type = type;
		}

		private void Nodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			Tree.Editor?.HasChanges = true;

			if (e.OldItems is not null)
			{
				foreach (var (propertyName, removedNode) in e.OldItems.OfType<NodeEntryViewModel>())
				{
					removedNode.Release();
					_nodeMap.Remove(propertyName);
				}
			}

			if (e.NewItems is not null)
			{
				foreach (var (propertyName, _) in e.NewItems.OfType<NodeEntryViewModel>())
				{
					_nodeMap.Add(propertyName, 0);
				}
			}

			for (var i = 0; i < Nodes.Count; i++)
			{
				_nodeMap[Nodes[i].PropertyName] = i;
			}
		}

		public override SchemaObjectType Type { get; }

		public ObservableCollection<NodeEntryViewModel> Nodes { get; }

		public override IEnumerable<NodeViewModelBase> Children => Nodes.Select((pair) => pair.Node);

		[RelayCommand]
		void Insert()
		{
			var number = 1;
			var generatedName = $"property1";

			while (_nodeMap.ContainsKey(generatedName))
			{
				generatedName = $"property{++number}";
			}

			var dialog = new StringValueDialog()
			{
				Owner = Tree.Editor!.OwnerWindow,
				Title = "Select Property Name",
				Text  = generatedName
			};

			if (dialog.ShowDialog() == true)
			{
				var propertyName = dialog.Text;
				if (_nodeMap.ContainsKey(propertyName))
				{
					Tree.Editor!.OwnerWindow.ShowMessage($"Map node: {Address}, already contains a property of name '{propertyName}'");
				}

				var newNode = Type.AdditionalPropertiesType!.Visit(new NodeViewModelGeneratorVisitor(Tree.Editor!.OwnerWindow, Tree, this, null));
				if (newNode is not null)
				{
					Nodes.Add(new NodeEntryViewModel()
					{
						PropertyName = propertyName, 
						Node         = newNode,
						IsAdditional = true,
					});
				}
			}
		}

		[RelayCommand]
		void CutNode(NodeEntryViewModel selectedItem)
		{
			Tree.Editor!.ClipboardNodes = new([selectedItem.Node.Clone(null)]);
			Nodes.RemoveAt(_nodeMap[selectedItem.PropertyName]);
		}

		[RelayCommand]
		void CopyNode(NodeEntryViewModel selectedItem)
		{
			Tree.Editor!.ClipboardNodes = new([selectedItem.Node.Clone(null)]);
		}

		[RelayCommand]
		void PasteNodes()
		{
			foreach (var node in Tree.Editor!.ClipboardNodes!.Where((node) => Type.AdditionalPropertiesType!.IsConvertibleFrom(node.Type)))
			{
				var dialog = new StringValueDialog()
				{
					Owner = Tree.Editor!.OwnerWindow,
					Text  = "Set Property Name"
				};

				if (dialog.ShowDialog() == true)
				{
					var propertyName = dialog.Text;
					if (_nodeMap.ContainsKey(propertyName))
					{
						Tree.Editor!.OwnerWindow.ShowMessage($"Map node: {Address}, already contains a property of name '{propertyName}'");
					}

					Nodes.Add(new NodeEntryViewModel()
					{
						PropertyName = propertyName,
						Node         = node.Clone(this),
						IsAdditional = true,
					});
				} 
			}
		}

		[RelayCommand]
		void DeleteNodes(IEnumerable selectedItems)
		{
			var nodesToRemove = selectedItems.OfType<NodeEntryViewModel>().ToList();
			foreach (var (propertyName, _) in nodesToRemove)
			{
				Nodes.RemoveAt(_nodeMap[propertyName]);
			}
		}

		public override ObjectNodeViewModel Clone(NodeViewModelBase? parent)
		{
			var result = new ObjectNodeViewModel(Tree, parent, Type);

			foreach (var entry in Nodes)
			{
				result.Nodes.Add(new NodeEntryViewModel()
				{
					PropertyName = entry.PropertyName, 
					Node         = entry.Node.Clone(result),
					IsAdditional = entry.IsAdditional
				});
			}

			//foreach (var property in Type.Properties)
			//{
			//	var entry = Nodes[property.Index];
			//	result.Nodes.Add(new NodeEntryViewModel()
			//	{
			//		PropertyName = property.Name, 
			//		Node         = Nodes[property.Index].Node.Clone(result),
			//		IsAdditional = entry.Ia
			//	});
			//}

			return result;
		}

		public override void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, int> referencedNodes)
		{
			writer.WriteStartObject();
			writer.WriteString("$type", Type.Name);

			foreach (var pair in Nodes)
			{
				writer.WritePropertyName(pair.PropertyName);
				pair.Node.SerializeTo(writer, referencedNodes);
			}
			writer.WriteEndObject();
		}

		protected override string GetName(NodeViewModelBase item)
		{
			if (Type.TryGetProperty("name", out var property) && Nodes[property.Index].Node is StringNodeViewModel stringNode)
			{
				return stringNode.Value;
			}

			foreach (var (propertyName, node) in Nodes)
			{
				if (node == item)
				{
					return propertyName;
				}
			}
			return "Child not found!";
		}
	}
}
