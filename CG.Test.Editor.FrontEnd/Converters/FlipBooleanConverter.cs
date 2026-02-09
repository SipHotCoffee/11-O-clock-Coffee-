namespace CG.Test.Editor.FrontEnd.Converters
{
    public class FlipBooleanConverter : ValueConverterBase<bool, bool>
    {
        public override bool Convert(bool source) => !source;

        public override bool ConvertBack(bool source) => !source;
    }
}
