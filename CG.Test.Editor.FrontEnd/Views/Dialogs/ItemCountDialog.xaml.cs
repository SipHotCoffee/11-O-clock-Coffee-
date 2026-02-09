using DependencyPropertyToolkit;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public partial class ItemCountDialog : CustomWindow
    {
		public ItemCountDialog()
        {
            InitializeComponent();

			MaximumItemCount = uint.MaxValue;

			ItemCount = 1;
		}

		[DependencyProperty]
		public partial uint ItemCount { get; set; }

		[DependencyProperty]
		public partial uint MaximumItemCount { get; set; }

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}
