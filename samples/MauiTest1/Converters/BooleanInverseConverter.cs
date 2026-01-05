using System.Globalization;

namespace MauiTest1.Converters;

public class BooleanInverseConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return value is bool boolValue && !boolValue;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return value is bool boolValue && !boolValue;
	}
}
