using GnomeMaui.CSS;

namespace Microsoft.Maui.Platform;

public static class CheckBoxExtensions
{
	private const string Prefix = "checkbox";

	public static Gtk.CheckButton Create(this CheckBoxHandler _)
	{
		var checkBox = new Gtk.CheckButton();
		checkBox.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
		return checkBox;
	}

	public static void UpdateText(this Gtk.CheckButton platform, ICheckBox checkBox)
	{
		//platform.Text = searchBar.Text ?? string.Empty;
	}
}
