using GnomeMaui.CSS;
using System;

namespace Microsoft.Maui.Platform;

public static class ActivityIndicatorExtensions
{
	private const string Prefix = "spinner";

	public static Gtk.Spinner Create(this ActivityIndicatorHandler handler)
	{
		var spinner = new Gtk.Spinner();
		return spinner;
	}

	public static void UpdateIsRunning(this Gtk.Spinner platformSpinner, IActivityIndicator activityIndicator)
	{
		if (activityIndicator.IsRunning)
			platformSpinner.Start();
		else
			platformSpinner.Stop();
	}

	public static void UpdateColor(this Gtk.Spinner platformSpinner, IActivityIndicator activityIndicator)
	{
		if (activityIndicator.Color == null)
		{
			return;
		}

		var color = activityIndicator.Color;
		var (r, g, b, a) = ((int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255), color.Alpha);
		var colorValue = $"rgba({r}, {g}, {b}, {a})";

		var instanceClass = CssCache.GetInstanceClass(platformSpinner);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ color: {colorValue}; }}");

		platformSpinner?.AddCssClass(instanceClass);
	}
}