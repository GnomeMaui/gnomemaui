using GnomeMaui.CSS;

namespace Microsoft.Maui.Platform;

public static class PickerExtensions
{
	private const string Prefix = "combobox";

	public static Gtk.ComboBoxText Create(this PickerHandler _)
	{
		var comboBox = new Gtk.ComboBoxText();
		comboBox.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
		return comboBox;
	}

	public static void UpdateItems(this Gtk.ComboBoxText platformPicker, IPicker picker)
	{
		// Remove all existing items
		platformPicker.RemoveAll();

		// Add all items from the picker
		if (picker.Items != null)
		{
			foreach (var item in picker.Items)
			{
				platformPicker.AppendText(item ?? string.Empty);
			}
		}

		// Restore the selected index
		UpdateSelectedIndex(platformPicker, picker);
	}

	public static void UpdateTitle(this Gtk.ComboBoxText platformPicker, IPicker picker)
	{
		// GTK ComboBoxText doesn't have a direct title/placeholder property
		// This would require a custom widget or overlay label
	}

	public static void UpdateTitleColor(this Gtk.ComboBoxText platformPicker, IPicker picker)
	{
		// GTK ComboBoxText doesn't have a direct title color property
	}

	public static void UpdateSelectedIndex(this Gtk.ComboBoxText platformPicker, IPicker picker)
	{
		if (picker.SelectedIndex >= 0 && picker.SelectedIndex < (picker.Items?.Count ?? 0))
		{
			platformPicker.Active = picker.SelectedIndex;
		}
		else
		{
			platformPicker.Active = -1;
		}
	}

	public static void UpdateCharacterSpacing(this Gtk.ComboBoxText platformPicker, IPicker picker)
	{
		var instanceClass = CssCache.GetInstanceClass(platformPicker);
		var spacing = picker.CharacterSpacing;
		var spacingValue = $"{spacing}px";

		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ letter-spacing: {spacingValue}; }}");
		platformPicker.AddCssClass(instanceClass);
	}

	public static void UpdateFont(this Gtk.ComboBoxText platformPicker, IPicker picker)
	{
		var instanceClass = CssCache.GetInstanceClass(platformPicker);
		var cssRules = new System.Text.StringBuilder();

		// Font family
		if (!string.IsNullOrEmpty(picker.Font.Family))
		{
			var fontFamilyValue = $"\"{picker.Font.Family}\"";
			cssRules.Append($"font-family: {fontFamilyValue}; ");
		}

		// Font size
		var size = picker.Font.Size;
		if (size > 0)
		{
			var fontSizeValue = $"{size}px";
			cssRules.Append($"font-size: {fontSizeValue}; ");
		}

		// Font weight (Bold)
		if (picker.Font.Weight >= FontWeight.Bold)
		{
			cssRules.Append("font-weight: bold; ");
		}

		// Font style (Italic)
		if (picker.Font.Slant == FontSlant.Italic)
		{
			cssRules.Append("font-style: italic; ");
		}

		if (cssRules.Length > 0)
		{
			var instanceSelector = $"{Prefix}.{instanceClass}";
			CssCache.AddElementSelector($"{instanceSelector} {{ {cssRules} }}");
			platformPicker.AddCssClass(instanceClass);
		}
	}

	public static void UpdateTextColor(this Gtk.ComboBoxText platformPicker, IPicker picker)
	{
		if (picker.TextColor == null)
		{
			return;
		}

		var color = picker.TextColor;
		var (r, g, b, a) = ((int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255), color.Alpha);
		var colorValue = $"rgba({r}, {g}, {b}, {a})";

		var instanceClass = CssCache.GetInstanceClass(platformPicker);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ color: {colorValue}; }}");

		platformPicker?.AddCssClass(instanceClass);
	}

	public static void UpdateHorizontalTextAlignment(this Gtk.ComboBoxText platformPicker, IPicker picker)
	{
		// GTK ComboBoxText doesn't have direct text alignment control
		// This would require custom cell renderer configuration
	}

	public static void UpdateVerticalTextAlignment(this Gtk.ComboBoxText platformPicker, IPicker picker)
	{
		// GTK ComboBoxText doesn't have direct vertical alignment control
	}

	public static void UpdateIsOpen(this Gtk.ComboBoxText platformPicker, IPicker picker)
	{
		if (picker.IsOpen)
		{
			platformPicker.Popup();
		}
		else
		{
			platformPicker.Popdown();
		}
	}
}
