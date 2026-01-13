using GnomeMaui.CSS;

namespace Microsoft.Maui.Platform;

public static class DatePickerExtensions
{
	private const string Prefix = "datepicker";

	public static Gtk.Calendar Create(this DatePickerHandler _)
	{
		var datePicker = new Gtk.Calendar();
		datePicker.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
		return datePicker;
	}

	public static void UpdateText(this Gtk.Calendar platform, IDatePicker datePicker)
	{
		//platform.Text = searchBar.Text ?? string.Empty;
	}
}
