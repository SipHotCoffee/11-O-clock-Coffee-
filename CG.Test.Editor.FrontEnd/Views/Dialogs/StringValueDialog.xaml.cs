using DependencyPropertyToolkit;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public partial class StringValueDialog : CustomWindow
    {
		public StringValueDialog()
        {
            InitializeComponent();
			_textBox.Focus();
        }

		[DependencyProperty]
		public partial int MaximumLength { get; set; }

		[DependencyProperty]
		public partial string Text { get; set; }

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}
