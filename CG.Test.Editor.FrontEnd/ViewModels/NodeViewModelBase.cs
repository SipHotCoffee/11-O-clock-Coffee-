using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Numerics;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public abstract partial class NodeViewModelBase(FileInstanceViewModel editor, NodeViewModelBase? parent) : ObservableObject, IEquatable<NodeViewModelBase>, IEqualityOperators<NodeViewModelBase, NodeViewModelBase, bool>
    {
		[ObservableProperty]
		private NodeViewModelBase? _selectedNode;

        public NodeViewModelBase Root => Parent?.Root ?? this;

        public NodePath Address
        {
            get
            {
                if (Parent is null)
                {
					return NodePath.Root;
				}

                if (Parent is ArrayNodeViewModel arrayNode)
                {
                    for (var i = 0; i < arrayNode.Elements.Count; i++)
                    {
                        if (arrayNode.Elements[i] == this)
                        {
							return Parent.Address.GetChild(new IndexIdentifier(i));
						}
                    }
                }

                if (Parent is ObjectNodeViewModel objectNode)
                {
                    foreach (var (name, node) in objectNode.Nodes)
                    {
                        if (node == this)
                        {
							return Parent.Address.GetChild(new NameIdentifier(name));
						}
                    }
                }

                return NodePath.Root;
            }
        }

		public FileInstanceViewModel Editor { get; } = editor;

        public NodeViewModelBase? Parent { get; private set; } = parent;

        public string Name => Parent?.GetName(this) ?? "(Root)";

        public abstract LinkedSchemaTypeBase Type { get; }

        public virtual IEnumerable<NodeViewModelBase> Children { get; } = [];

        public IEnumerable<NodeViewModelBase> AllChildren => Children.Concat(Children.SelectMany((child) => child.AllChildren));

        public abstract NodeViewModelBase Clone(NodeViewModelBase? parent);

        public abstract void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, ulong> referencedNodes);

		protected abstract string GetName(NodeViewModelBase item);

		[RelayCommand]
		void Navigate()
		{
			Editor.Navigate(this);
		}

        public bool Equals(NodeViewModelBase? other) => Equals(this, other);

		public static bool Equals(NodeViewModelBase? leftNode, NodeViewModelBase? rightNode)
        {
            if (ReferenceEquals(leftNode, rightNode))
            {
                return true;
            }

            var left = leftNode;
            if (left is VariantNodeViewModel leftVariantNode)
            {
                left = leftVariantNode.SelectedObject;
            }

			var right = rightNode;
			if (right is VariantNodeViewModel rightVariantNode)
			{
				right = rightVariantNode.SelectedObject;
			}

            return ReferenceEquals(left, right);
		}

        public static bool operator ==(NodeViewModelBase? left, NodeViewModelBase? right) => Equals(left, right);
        public static bool operator !=(NodeViewModelBase? left, NodeViewModelBase? right) => !Equals(left, right);

        public override bool Equals(object? obj) => Equals(obj as NodeViewModelBase);

        public override int GetHashCode()
        {
            if (this is VariantNodeViewModel variantNode)
            {
                return variantNode.SelectedObject.GetHashCode();
            }
            return base.GetHashCode();
        }
    }
}
