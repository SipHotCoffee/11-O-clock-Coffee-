using CG.Test.Editor.FrontEnd.Models;
using DependencyPropertyToolkit;
using System.Collections.ObjectModel;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for VariantTypeSelectorDialog.xaml
    /// </summary>
    public partial class VariantTypeSelectorDialog : CustomWindow
    {
		public VariantTypeSelectorDialog()
        {
            InitializeComponent();
        }

        [DependencyProperty]
		public partial ObservableCollection<SchemaObjectType> PossibleTypes { get; set; }

		[DependencyProperty]
		public partial SchemaObjectType SelectedType { get; set; }

		private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
			Close();
		}
	}
}
