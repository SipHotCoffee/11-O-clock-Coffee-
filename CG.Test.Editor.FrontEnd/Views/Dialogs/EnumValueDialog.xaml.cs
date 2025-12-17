using DependencyPropertyToolkit;
using System.Collections.ObjectModel;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public partial class EnumValueDialog : CustomWindow
    {
		public EnumValueDialog()
        {
            InitializeComponent();
        }

        [DependencyProperty]
        public partial int SelectedIndex { get; set; }

        [DependencyProperty]
		public partial string SelectedItem { get; set; }

        [DependencyProperty]
		public partial ObservableCollection<string> PossibleValues { get; set; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
		}
	}
}
