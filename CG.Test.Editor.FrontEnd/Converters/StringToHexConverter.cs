using System.Globalization;

namespace CG.Test.Editor.FrontEnd.Converters
{
    public class StringToHexConverter : ValueConverterBase<byte, string>
    {
        public override string Convert(byte source) => System.Convert.ToString(source, 16).ToUpper();

        public override byte ConvertBack(string stringValue)
        {
			if (!int.TryParse(stringValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var intValue))
			{
				return 0;
			}

			if (intValue > byte.MaxValue)
			{
				return byte.Parse(stringValue.AsSpan(0, 2), NumberStyles.HexNumber);
			}

			return (byte)intValue;
		}
    }
}
