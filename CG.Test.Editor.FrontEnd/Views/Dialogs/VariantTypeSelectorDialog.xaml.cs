using CG.Test.Editor.FrontEnd.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for VariantTypeSelectorDialog.xaml
    /// </summary>
    public partial class VariantTypeSelectorDialog : CustomWindow
    {
		public static readonly DependencyProperty PossibleTypesProperty = DependencyProperty.Register(nameof(PossibleTypes), typeof(ObservableCollection<SchemaObjectType>), typeof(VariantTypeSelectorDialog));
		public static readonly DependencyProperty SelectedTypeProperty = DependencyProperty.Register(nameof(SelectedType), typeof(SchemaObjectType), typeof(VariantTypeSelectorDialog));

		public VariantTypeSelectorDialog()
        {
            InitializeComponent();
        }

		public ObservableCollection<SchemaObjectType> PossibleTypes
		{
			get => (ObservableCollection<SchemaObjectType>)GetValue(PossibleTypesProperty);
			set => SetValue(PossibleTypesProperty, value);
		}

		public SchemaObjectType SelectedType
		{
			get => (SchemaObjectType)GetValue(SelectedTypeProperty);
			set => SetValue(SelectedTypeProperty, value);
		}

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
			Close();
		}
	}
}
