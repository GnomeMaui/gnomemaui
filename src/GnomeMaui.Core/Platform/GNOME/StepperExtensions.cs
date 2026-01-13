using GnomeMaui.CSS;

namespace Microsoft.Maui.Platform;

public static class StepperExtensions
{
	private const string Prefix = "stepper";

	public static Gtk.SpinButton Create(this StepperHandler _)
	{
		var stepper = new Gtk.SpinButton();
		stepper.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
		return stepper;
	}

	public static void UpdateText(this Gtk.SpinButton platform, IStepper stepper)
	{
		//platform.Text = searchBar.Text ?? string.Empty;
	}
}
