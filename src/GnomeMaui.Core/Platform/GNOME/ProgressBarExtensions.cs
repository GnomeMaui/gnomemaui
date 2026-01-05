using GnomeMaui.CSS;

namespace Microsoft.Maui.Platform;

public static class ProgressBarExtensions
{
	private const string Prefix = "progressbar";

	public static Gtk.ProgressBar Create(this ProgressBarHandler _)
	{
		var progressBar = new Gtk.ProgressBar();
		progressBar.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");

		var instanceClass = CssCache.GetInstanceClass(progressBar);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		// TODO: Make margin configurable
		CssCache.AddElementSelector($"{instanceSelector} {{ margin-top: 12px; margin-bottom: 12px; }}");

		return progressBar;
	}

	public static void UpdateProgress(this Gtk.ProgressBar platformProgressBar, IProgress progress)
	{
		// Clamp progress value between 0 and 1
		var fraction = progress.Progress;
		if (fraction < 0)
			fraction = 0;
		else if (fraction > 1)
			fraction = 1;

		platformProgressBar.Fraction = fraction;
	}

	public static void UpdateProgressColor(this Gtk.ProgressBar platformProgressBar, IProgress progress)
	{
		if (progress.ProgressColor == null)
		{
			return;
		}

		var color = progress.ProgressColor;
		var (r, g, b, a) = ((int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255), color.Alpha);
		var colorValue = $"rgba({r}, {g}, {b}, {a})";

		var instanceClass = CssCache.GetInstanceClass(platformProgressBar);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		// Target the progress fill (trough is the background)
		CssCache.AddElementSelector($"{instanceSelector} > trough > progress {{ background-color: {colorValue}; }}");

		platformProgressBar?.AddCssClass(instanceClass);
	}
}
