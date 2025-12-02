using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CG.Test.Editor.FrontEnd.Views.Dialogs
{
    public partial class StringValueDialog : Window
    {
		public static readonly DependencyProperty    MaximumLengthProperty = DependencyProperty.Register(nameof(MaximumLength), typeof(int), typeof(StringValueDialog));
		public static readonly DependencyProperty             TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(StringValueDialog));

		public StringValueDialog()
        {
            InitializeComponent();
        }

		public int MaximumLength
		{
			get => (int)GetValue(MaximumLengthProperty);
			set => SetValue(MaximumLengthProperty, value);
		}

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}
	}
}
