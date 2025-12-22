using CG.Test.Editor.FrontEnd.Models;
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
            throw new NotImplementedException();
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
			return ((SchemaObjectType)value)?.Properties.Where((property) => property.Type.IsValueType);
		}
    }

    public partial class RepairDialog : CustomWindow
    {
        public RepairDialog()
        {
            InitializeComponent();
        }

		[DependencyProperty]
		public partial ObservableCollection<SchemaObjectType> AvailableTypes { get; set; }

        [DependencyProperty]
        public partial IEnumerable<SchemaProperty> Properties { get; set; }
	}
}
