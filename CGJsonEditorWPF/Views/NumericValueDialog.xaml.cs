using System.Windows;

namespace CG.Test.Editor.Json.Dialogs
{
    public partial class NumericValueDialog : CustomWindow
    {
        public NumericValueDialog()
        {
            InitializeComponent();
        }

        public bool IsIntegral
        {
            get => _spinButton.IsIntegral;
            set => _spinButton.IsIntegral = value;
        }

        public double Minimum
        {
            get => _spinButton.Minimum;
            set => _spinButton.Minimum = value;
        }

        public double Maximum
        {
            get => _spinButton.Maximum;
            set => _spinButton.Maximum = value;
        }

        public double Value
        {
            get => _spinButton.Value;
            set => _spinButton.Value = value;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
