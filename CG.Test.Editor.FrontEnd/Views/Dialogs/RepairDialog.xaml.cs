using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CG.Test.Editor.FrontEnd.Visitors;
using DependencyPropertyToolkit;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public class FieldValueFilterConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
			return ((IEnumerable<LinkedSchemaProperty>)value)?.Where((property) => property.Type.IsValueType);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
			throw new NotImplementedException();
		}
    }

    public partial class RepairDialog : CustomWindow
    {
        private readonly FileInstanceViewModel _instanceViewModel;
        
        private readonly NodeViewModelBase? _parent;

        public RepairDialog(FileInstanceViewModel instanceViewModel, NodeViewModelBase? parent)
        {
            InitializeComponent();

            _instanceViewModel = instanceViewModel;

            _parent = parent;
        }

		[DependencyProperty]
		public partial ObservableCollection<LinkedSchemaObjectType> AvailableTypes { get; set; }

        [DependencyProperty]
        public partial LinkedSchemaObjectType SelectedType { get; set; }

        [DependencyProperty]
        public partial IEnumerable<LinkedSchemaProperty> Properties { get; set; }

        [DependencyProperty]
        public partial LinkedSchemaProperty SelectedProperty { get; set; }

        [DependencyProperty]
        public partial bool IsFilterEnabled { get; set; }

        [DependencyProperty]
        public partial NodeViewModelBase OldValue { get; set; }

		[DependencyProperty]
		public partial NodeViewModelBase NewValue { get; set; }

        partial void OnSelectedPropertyChanged(LinkedSchemaProperty oldValue, LinkedSchemaProperty newValue)
        {
            var visitor = new NodeViewModelGeneratorVisitor(_instanceViewModel, _parent, null);

			OldValue = newValue.Type.Visit(visitor);
			NewValue = newValue.Type.Visit(visitor);
		}

        private void OldValueButton_Click(object sender, RoutedEventArgs e)
        {
            var visitor = new NodeEditorVisitor(_instanceViewModel, true);
            OldValue.Visit(visitor);
		}

        private void NewValueButton_Click(object sender, RoutedEventArgs e)
        {
			var visitor = new NodeEditorVisitor(_instanceViewModel, true);
			NewValue.Visit(visitor);
		}

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
		}

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            var occurences = 0;

			foreach (var child in _instanceViewModel.Current!.AllChildren)
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
