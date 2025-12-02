using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CG.Test.Editor.FrontEnd.Views.Controls
{
    public partial class SpinButton : UserControl
    {
		public static readonly DependencyProperty      ValueProperty = DependencyProperty.Register(nameof(Value)  ,    typeof(double), typeof(SpinButton), new FrameworkPropertyMetadata(0.0, null, LimitValueCallBack));
		public static readonly DependencyProperty    MinimumProperty = DependencyProperty.Register(nameof(Minimum),    typeof(double), typeof(SpinButton));
		public static readonly DependencyProperty    MaximumProperty = DependencyProperty.Register(nameof(Maximum),    typeof(double), typeof(SpinButton));
		public static readonly DependencyProperty       StepProperty = DependencyProperty.Register(nameof(Step),       typeof(double), typeof(SpinButton));
		public static readonly DependencyProperty IsIntegralProperty = DependencyProperty.Register(nameof(IsIntegral), typeof(bool)  , typeof(SpinButton));

		static SpinButton()
		{
			// Link this control to its default style in Generic.xaml
			DefaultStyleKeyProperty.OverrideMetadata
			(
				typeof(SpinButton),
				new FrameworkPropertyMetadata(typeof(SpinButton))
			);
		}

		private bool _isIntegral;

		public SpinButton()
		{
			InitializeComponent();

			Minimum = 0;
			Maximum = 100;

			Value = 0;

			Step = 1;

			_isIntegral = false;
		}

		public double Value
		{
			get => (double)GetValue(ValueProperty);
			set
			{
				var clampedValue = Math.Clamp(value, Minimum, Maximum);
				if (_isIntegral)
				{
					clampedValue = Math.Truncate(clampedValue);
				}

				SetValue(ValueProperty, clampedValue);
			}
		}

		public double Minimum
		{
			get => (double)GetValue(MinimumProperty);
			set
			{
				var minimum = value;
				if (_isIntegral)
				{
					minimum = Math.Truncate(minimum);
				}
				SetValue(MinimumProperty, minimum);
			}
		}

		public double Maximum
		{
			get => (double)GetValue(MaximumProperty);
			set
			{
				var maximum = value;
				if (_isIntegral)
				{
					maximum = Math.Truncate(maximum);
				}
				SetValue(MaximumProperty, maximum);
			}
		}

		public double Step
		{
			get => (double)GetValue(StepProperty);
			set => SetValue(StepProperty, value);
		}

		public bool IsIntegral
		{
			get => _isIntegral;
			set
			{
				_isIntegral = value;
				if (value)
				{
					Value = Math.Truncate(Math.Clamp(Value, Minimum, Maximum));
				}
			}
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (GetTemplateChild("PART_IncrementButton") is RepeatButton incrementButton)
			{
				incrementButton.Click += IncrementButton_Click;
			}

			if (GetTemplateChild("PART_DecrementButton") is RepeatButton decrementButton)
			{
				decrementButton.Click += DecrementButton_Click;
			}

			//if (GetTemplateChild("PART_ValueTextBox") is TextBox textBox)
			//{
			//    textBox.TextChanged += (sender, e) =>
			//    {
			//        if (!double.TryParse(textBox.Text, out var value))
			//        {

			//        }
			//    };
			//}
		}

		public void IncrementButton_Click(object sender, RoutedEventArgs e)
		{
			Value += Step;
		}

		public void DecrementButton_Click(object sender, RoutedEventArgs e)
		{
			Value -= Step;
		}

		private static object LimitValueCallBack(DependencyObject dependencyObject, object baseValue)
		{
			var value = (double)baseValue;
			return Math.Clamp(value, (double)dependencyObject.GetValue(MinimumProperty), (double)dependencyObject.GetValue(MaximumProperty));
		}
	}
}
