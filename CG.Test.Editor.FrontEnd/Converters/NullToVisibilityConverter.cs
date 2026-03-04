using System.Windows;

namespace CG.Test.Editor.FrontEnd.Converters
{
    public class NullToVisibilityConverter : ValueConverterBase<object?, Visibility>
    {
		public override Visibility Convert(object? source)
		{
			return source is null ? Visibility.Collapsed : Visibility.Visible;
		}

		public override object? ConvertBack(Visibility source)
		{
			throw new NotImplementedException();
		}
	}
}
