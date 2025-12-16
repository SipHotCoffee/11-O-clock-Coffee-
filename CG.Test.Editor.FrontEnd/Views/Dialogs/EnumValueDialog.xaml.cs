using System.Collections.ObjectModel;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public partial class EnumValueDialog : CustomWindow
    {
        public static readonly DependencyProperty  SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(EnumValueDialog));
		public static readonly DependencyProperty   SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(string), typeof(EnumValueDialog));
		public static readonly DependencyProperty PossibleValuesProperty = DependencyProperty.Register(nameof(PossibleValues), typeof(ObservableCollection<string>), typeof(EnumValueDialog));

		public EnumValueDialog()
        {
            InitializeComponent();
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

		public string SelectedItem
		{
			get => (string)GetValue(SelectedItemProperty);
			set => SetValue(SelectedItemProperty, value);
		}

		public ObservableCollection<string> PossibleValues
        {
            get => (ObservableCollection<string>)GetValue(PossibleValuesProperty);
            set => SetValue(PossibleValuesProperty, value);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
		}
	}
}
