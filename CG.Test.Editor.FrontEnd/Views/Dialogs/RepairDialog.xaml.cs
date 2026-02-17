using CG.Test.Editor.FrontEnd.Models.Types;
using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CG.Test.Editor.FrontEnd.Visitors;
using DependencyPropertyToolkit;
using System.Collections.ObjectModel;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public class FieldValueFilterConverter : ValueConverterBase<IEnumerable<LinkedSchemaProperty>, IEnumerable<LinkedSchemaProperty>>
    {
        public override IEnumerable<LinkedSchemaProperty> Convert(IEnumerable<LinkedSchemaProperty> properties) => properties.Where((property) => property.Type.IsValueType);

        public override IEnumerable<LinkedSchemaProperty> ConvertBack(IEnumerable<LinkedSchemaProperty> source)
        {
            throw new NotImplementedException();
        }
    }

    public partial class RepairDialog : CustomWindow
    {
        private readonly NodeTree _tree;
        
        private readonly NodeViewModelBase? _parent;

        public RepairDialog(NodeTree tree, NodeViewModelBase? parent)
        {
            InitializeComponent();

			_tree = tree;

            _parent = parent;
        }

		[DependencyProperty]
		public partial ObservableCollection<SchemaObjectType> AvailableTypes { get; set; }

        [DependencyProperty]
        public partial SchemaObjectType SelectedType { get; set; }

        [DependencyProperty]
        public partial IEnumerable<LinkedSchemaProperty> Properties { get; set; }

        [DependencyProperty]
        public partial LinkedSchemaProperty SelectedProperty { get; set; }

        [DependencyProperty]
        public partial bool IsFilterEnabled { get; set; }

        [DependencyProperty]
        public partial NodeViewModelBase? OldValue { get; set; }

		[DependencyProperty]
		public partial NodeViewModelBase? NewValue { get; set; }

        partial void OnSelectedPropertyChanged(LinkedSchemaProperty oldValue, LinkedSchemaProperty newValue)
        {
            var visitor = new NodeViewModelGeneratorVisitor(this, _tree, _parent, null);

			OldValue = newValue.Type.Visit(visitor)!;
			NewValue = newValue.Type.Visit(visitor)!;
		}

        private void OldValueButton_Click(object sender, RoutedEventArgs e)
        {
            var visitor = new NodeEditorVisitor(true);
            OldValue?.Visit(visitor);
		}

        private void NewValueButton_Click(object sender, RoutedEventArgs e)
        {
			var visitor = new NodeEditorVisitor(true);
			NewValue?.Visit(visitor);
		}

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
		}

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewValue is null)
            {
                return;
            }

            var occurences = 0;

			foreach (var child in _tree.Editor!.Current!.AllChildren)
			{
				if (child is ObjectNodeViewModel childObject && childObject.Type == SelectedType)
				{
                    var node = childObject.Nodes[SelectedProperty.Index].Value;
                    if (!IsFilterEnabled || node.Equals(OldValue))
                    {
                        ++occurences;
						node.Visit(new NodeAssignerVisitor(NewValue));
                    }
				}
			}

            this.ShowMessage($"{occurences} occurences replaced.");
		}
	}
}
