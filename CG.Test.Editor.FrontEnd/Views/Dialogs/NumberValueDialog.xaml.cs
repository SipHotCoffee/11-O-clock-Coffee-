using DependencyPropertyToolkit;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public partial class NumberValueDialog : CustomWindow
    {
		public NumberValueDialog()
        {
            InitializeComponent();
        }

		[DependencyProperty]
		public partial bool IsIntegral { get; set; }

		[DependencyProperty]
		public partial double Minimum { get; set; }

		[DependencyProperty]
		public partial double Maximum { get; set; }

		[DependencyProperty]
		public partial double Value { get; set; }

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			Value = Math.Clamp(Value, Minimum, Maximum);
			DialogResult = true;
			Close();
		}
	}
}
