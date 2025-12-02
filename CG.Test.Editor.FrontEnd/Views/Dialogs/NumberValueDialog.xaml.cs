using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for NumberValueDialog.xaml
    /// </summary>
    public partial class NumberValueDialog : CustomWindow
    {
		public static readonly DependencyProperty IsIntegralProperty = DependencyProperty.Register(nameof(IsIntegral), typeof(bool), typeof(NumberValueDialog));
		public static readonly DependencyProperty    MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumberValueDialog));
		public static readonly DependencyProperty    MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumberValueDialog));
		public static readonly DependencyProperty      ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumberValueDialog));

		public NumberValueDialog()
        {
            InitializeComponent();
        }

		public bool IsIntegral
		{
			get => (bool)GetValue(IsIntegralProperty);
			set => SetValue(IsIntegralProperty, value);
		}

		public double Minimum
		{
			get => (double)GetValue(MinimumProperty);
			set => SetValue(MinimumProperty, value);
		}

		public double Maximum
		{
			get => (double)GetValue(MaximumProperty);
			set => SetValue(MaximumProperty, value);
		}

		public double Value
		{
			get => (double)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			Value = Math.Clamp(Value, Minimum, Maximum);
			DialogResult = true;
			Close();
		}
	}
}
