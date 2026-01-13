#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.GNOMESpecific;

/// <summary>
/// GNOME-specific configuration for Shell.
/// </summary>
public static class Shell
{
	/// <summary>
	/// Backing store for the attached property that controls the
	/// auto-flyout breakpoint width.
	/// </summary>
	public static readonly BindableProperty AutoFlyoutProperty =
		BindableProperty.Create("AutoFlyout",
		typeof(int),
		typeof(Shell),
		500,
		propertyChanged: OnAutoFlyoutChanged);

	/// <summary>
	/// Returns the auto-flyout breakpoint width in pixels (0 = disabled).
	/// </summary>
	/// <param name="element">The platform specific element on which to
	/// perform the operation.</param>
	/// <returns>The breakpoint width in pixels.</returns>
	public static int GetAutoFlyout(BindableObject element)
		=> (int)element.GetValue(AutoFlyoutProperty);

	/// <summary>
	/// Sets the auto-flyout breakpoint width in pixels (0 = disabled).
	/// When window width is below this value, flyout will automatically switch to overlay mode.
	/// When above, it will switch to side-by-side mode.
	/// </summary>
	/// <param name="element">The platform specific element on which to perform the operation.</param>
	/// <param name="value">The breakpoint width in pixels.</param>
	public static void SetAutoFlyout(BindableObject element, int value)
		=> element.SetValue(AutoFlyoutProperty, value);

	private static void OnAutoFlyoutChanged(BindableObject bindable, object oldValue, object newValue)
	{
	}
}
