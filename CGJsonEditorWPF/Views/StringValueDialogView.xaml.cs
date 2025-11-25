using System.Windows;

namespace CG.Test.Editor.Json.Dialogs
{
    public partial class StringValueDialogView : CustomWindow
    {
        public StringValueDialogView()
        {
            InitializeComponent();
        }

        private void CustomWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _textBox.Focus();
        }
    }
}
