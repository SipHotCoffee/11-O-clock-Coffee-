using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace CG.Test.Editor.FrontEnd
{
    public abstract class ValueConverterBase<TSource, TTarget> : IValueConverter
    {
        public abstract TTarget Convert(TSource source);
        public abstract TSource ConvertBack(TTarget source);

        object? IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is not null ? Convert((TSource)value) : null;

        object? IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is not null ? ConvertBack((TTarget)value) : null;
	}

	public abstract class MultiValueConverterBase<TTarget> : IMultiValueConverter
	{
        object? IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any((item) => item == DependencyProperty.UnsetValue || item is null))
            {
                return null;
            }

            var method = GetType().GetMethod("Convert", BindingFlags.Instance | BindingFlags.Public);
            return method is not null ? method.Invoke(this, values) : throw new NotImplementedException();
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
			var method = GetType().GetMethod("ConvertBack", BindingFlags.Instance | BindingFlags.Public, [.. targetTypes.Select((targetType) => targetType.MakeByRefType()), typeof(TTarget)]) ?? throw new NotImplementedException();
            var arguments = new object[targetTypes.Length + 1];
            arguments[0] = value;
			method.Invoke(this, [ arguments ] );
            return arguments.AsSpan(1).ToArray();
		}
    }
}
