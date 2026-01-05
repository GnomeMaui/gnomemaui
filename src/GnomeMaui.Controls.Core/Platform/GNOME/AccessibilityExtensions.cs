#nullable disable

namespace Microsoft.Maui.Controls.Platform;

public static class AccessibilityExtensions
{
	public static void SetAccessibilityProperties(Gtk.Accessible accessible, Element element)
	{
		if (accessible == null || element == null)
			return;

#pragma warning disable CS0618
		var name = element.GetValue(AutomationProperties.NameProperty) as string;
		var helpText = element.GetValue(AutomationProperties.HelpTextProperty) as string;
#pragma warning restore CS0618

		if (!string.IsNullOrEmpty(name))
		{
			var value = new GObject.Value(name);
			var values = new GObject.Value[] { value };
			var properties = new Gtk.AccessibleProperty[] { Gtk.AccessibleProperty.Label };
			Gtk.Internal.Accessible.UpdateProperty(accessible.Handle.DangerousGetHandle(), 1, properties, GObject.Internal.ValueArray2OwnedHandle.Create(values));
		}

		if (!string.IsNullOrEmpty(helpText))
		{
			var value = new GObject.Value(helpText);
			var values = new GObject.Value[] { value };
			var properties = new Gtk.AccessibleProperty[] { Gtk.AccessibleProperty.HelpText };
			Gtk.Internal.Accessible.UpdateProperty(accessible.Handle.DangerousGetHandle(), 1, properties, GObject.Internal.ValueArray2OwnedHandle.Create(values));
		}
	}
}
