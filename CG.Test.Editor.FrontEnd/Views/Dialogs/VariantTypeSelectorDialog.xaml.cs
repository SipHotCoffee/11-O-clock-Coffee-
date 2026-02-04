using CG.Test.Editor.FrontEnd.Models.LinkedTypes;
using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using DependencyPropertyToolkit;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
		public partial ICollectionView PossibleTypes { get; set; }

		[DependencyProperty]
		public partial LinkedSchemaObjectType SelectedType { get; set; }

        [DependencyProperty]
        public partial string SearchText { get; set; }

        partial void OnSearchTextChanged(string oldValue, string newValue)
        {
			PossibleTypes.Filter = (value) =>
			{
				if (value is LinkedSchemaObjectType type)
				{
					return type.Name.Contains(newValue, StringComparison.CurrentCultureIgnoreCase);
				}

				if (value is LinkedSchemaSymbolType symbolType)
				{
					return symbolType.TypeName.Contains(newValue, StringComparison.CurrentCultureIgnoreCase);
				}
				return false;
			};
		}

		private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
			Close();
		}
	}
}
