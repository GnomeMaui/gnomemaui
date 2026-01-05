using System.Globalization;

namespace MauiTest1.Converters;

public class DoubleToIntConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		(int)Math.Round((value as double? ?? 0) * GetMultiplier(parameter));

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		(value as int? ?? 0) / GetMultiplier(parameter);

	private static double GetMultiplier(object? parameter) =>
		parameter is string s && double.TryParse(s, out var m) ? m : 1;
}