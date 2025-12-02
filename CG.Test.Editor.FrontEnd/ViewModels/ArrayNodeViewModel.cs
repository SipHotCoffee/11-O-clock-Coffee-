using CG.Test.Editor.FrontEnd.Models;
using System.Collections.ObjectModel;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
    public class ArrayNodeViewModel : NodeViewModelBase
    {
        public ObservableCollection<NodeViewModelBase> Elements { get; }
       
        public ObservableCollection<int> Indices { get; }

        public override SchemaArrayType Type { get; }

        public ArrayNodeViewModel(SchemaArrayType type)
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

        public override ArrayNodeViewModel Clone()
        {
            var result = new ArrayNodeViewModel(Type);
            foreach (var element in Elements)
            {
                result.Elements.Add(element.Clone());
            }
            return result;
        }

        
    }
}
