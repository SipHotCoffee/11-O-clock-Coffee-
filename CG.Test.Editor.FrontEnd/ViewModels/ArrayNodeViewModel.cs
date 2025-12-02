using CG.Test.Editor.FrontEnd.Models;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public partial class ArrayNodeViewModel : NodeViewModelBase
    {
        public ObservableCollection<NodeViewModelBase> Elements { get; }
       
        public ObservableCollection<int> Indices { get; }

        public override SchemaArrayType Type { get; }

        public ArrayNodeViewModel(NodeViewModelBase? parent, SchemaArrayType type) : base(parent)
        {
            Type = type;

            Elements = [];

            Indices = [];

            Elements.CollectionChanged += Elements_CollectionChanged;
		}

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
            var result = new ArrayNodeViewModel(parent, Type);
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
            Elements.Add(Type.ElementType.Visit(new NodeViewModelGeneratorVisitor(this)));
        }
    }
}
