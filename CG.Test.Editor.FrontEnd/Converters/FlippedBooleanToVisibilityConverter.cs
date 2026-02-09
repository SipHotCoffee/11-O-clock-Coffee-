using System.Windows;

namespace CG.Test.Editor.FrontEnd.Converters
{
    public class FlippedBooleanToVisibilityConverter : ValueConverterBase<bool, Visibility>
    {
        public override Visibility Convert(bool source) => source ? Visibility.Collapsed : Visibility.Visible;

		public override bool ConvertBack(Visibility visibility) => visibility == Visibility.Collapsed;
    }
}
