using GnomeMaui.CSS;

namespace Microsoft.Maui.Platform;

public static class RadioButtonExtensions
{
	private const string Prefix = "radiobutton";

	public static Gtk.CheckButton Create(this RadioButtonHandler _)
	{
		var radioButton = new Gtk.CheckButton();
		radioButton.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
		return radioButton;
	}

	public static void UpdateText(this Gtk.CheckButton platform, IRadioButton radioButton)
	{
		//platform.Text = searchBar.Text ?? string.Empty;
	}
}
