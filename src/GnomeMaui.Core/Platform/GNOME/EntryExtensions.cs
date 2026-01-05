using GnomeMaui.CSS;

namespace Microsoft.Maui.Platform;

public static class EntryExtensions
{
	private const string Prefix = "entry";

	public static Gtk.Entry Create(this EntryHandler _)
	{
		var entry = new Gtk.Entry();
		entry.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
		return entry;
	}

	public static void UpdateText(this Gtk.Entry platformEntry, IEntry entry)
	{
		platformEntry.Text_ = entry.Text ?? string.Empty;
	}

	public static void UpdateTextColor(this Gtk.Entry platformEntry, IEntry entry)
	{
		if (entry.TextColor == null)
		{
			return;
		}

		var color = entry.TextColor;
		var (r, g, b, a) = ((int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255), color.Alpha);
		var colorValue = $"rgba({r}, {g}, {b}, {a})";

		var instanceClass = CssCache.GetInstanceClass(platformEntry);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ color: {colorValue}; }}");

		platformEntry?.AddCssClass(instanceClass);
	}

	public static void UpdateIsPassword(this Gtk.Entry platformEntry, IEntry entry)
	{
		platformEntry.Visibility = !entry.IsPassword;
	}

	public static void UpdateHorizontalTextAlignment(this Gtk.Entry platformEntry, IEntry entry)
	{
		var xalign = entry.HorizontalTextAlignment switch
		{
			TextAlignment.Start => 0.0f,
			TextAlignment.Center => 0.5f,
			TextAlignment.End => 1.0f,
			_ => 0.0f
		};
		platformEntry.Xalign = xalign;
	}

	public static void UpdateVerticalTextAlignment(this Gtk.Entry platformEntry, IEntry entry)
	{
		// Gtk.Entry doesn't have direct vertical alignment support
		// This would need custom rendering or container alignment
	}

	public static void UpdateIsTextPredictionEnabled(this Gtk.Entry platformEntry, IEntry entry)
	{
		platformEntry.InputPurpose = entry.IsTextPredictionEnabled
			? Gtk.InputPurpose.FreeForm
			: Gtk.InputPurpose.Terminal;
	}

	public static void UpdateIsSpellCheckEnabled(this Gtk.Entry platformEntry, IEntry entry)
	{
		platformEntry.EnableEmojiCompletion = entry.IsSpellCheckEnabled;
	}

	public static void UpdateMaxLength(this Gtk.Entry platformEntry, IEntry entry)
	{
		if (entry.MaxLength > 0)
		{
			platformEntry.MaxLength = entry.MaxLength;
		}
		else
		{
			platformEntry.MaxLength = 0; // 0 means no limit in GTK
		}
	}

	public static void UpdatePlaceholder(this Gtk.Entry platformEntry, IEntry entry)
	{
		platformEntry.PlaceholderText = entry.Placeholder ?? string.Empty;
	}

	public static void UpdatePlaceholderColor(this Gtk.Entry platformEntry, IEntry entry)
	{
		if (entry.PlaceholderColor == null)
		{
			return;
		}

		var color = entry.PlaceholderColor;
		var (r, g, b, a) = ((int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255), color.Alpha);
		var colorValue = $"rgba({r}, {g}, {b}, {a})";

		var instanceClass = CssCache.GetInstanceClass(platformEntry);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} ::placeholder {{ color: {colorValue}; }}");

		platformEntry?.AddCssClass(instanceClass);
	}

	public static void UpdateIsReadOnly(this Gtk.Entry platformEntry, IEntry entry)
	{
		platformEntry.Editable = !entry.IsReadOnly;
	}

	public static void UpdateKeyboard(this Gtk.Entry platformEntry, IEntry entry)
	{
		var keyboard = entry.Keyboard;

		platformEntry.InputPurpose = keyboard switch
		{
			Keyboard k when k == Keyboard.Email => Gtk.InputPurpose.Email,
			Keyboard k when k == Keyboard.Numeric => Gtk.InputPurpose.Number,
			Keyboard k when k == Keyboard.Telephone => Gtk.InputPurpose.Phone,
			Keyboard k when k == Keyboard.Url => Gtk.InputPurpose.Url,
			_ => Gtk.InputPurpose.FreeForm
		};
	}

	public static void UpdateFont(this Gtk.Entry platformEntry, IEntry entry)
	{
		var instanceClass = CssCache.GetInstanceClass(platformEntry);
		var cssRules = new System.Text.StringBuilder();

		// Font family
		if (!string.IsNullOrEmpty(entry.Font.Family))
		{
			var fontFamilyValue = $"\"{entry.Font.Family}\"";
			cssRules.Append($"font-family: {fontFamilyValue}; ");
		}

		// Font size
		var size = entry.Font.Size;
		if (size > 0)
		{
			var fontSizeValue = $"{size}px";
			cssRules.Append($"font-size: {fontSizeValue}; ");
		}

		// Font weight (Bold)
		if (entry.Font.Weight >= FontWeight.Bold)
		{
			cssRules.Append("font-weight: bold; ");
		}

		// Font style (Italic)
		if (entry.Font.Slant == FontSlant.Italic)
		{
			cssRules.Append("font-style: italic; ");
		}

		if (cssRules.Length > 0)
		{
			var instanceSelector = $"{Prefix}.{instanceClass}";
			CssCache.AddElementSelector($"{instanceSelector} {{ {cssRules} }}");
			platformEntry.AddCssClass(instanceClass);
		}
	}

	public static void UpdateReturnType(this Gtk.Entry platformEntry, IEntry entry)
	{
		// GTK Entry activates when Enter is pressed, but doesn't have different return key types
		// The Completed event can be hooked up via the Activate signal in the handler
	}

	public static void UpdateClearButtonVisibility(this Gtk.Entry platformEntry, IEntry entry)
	{
		platformEntry.SetIconFromIconName(Gtk.EntryIconPosition.Secondary,
			entry.ClearButtonVisibility == ClearButtonVisibility.WhileEditing
				? "edit-clear-symbolic"
				: null);
	}

	public static void UpdateCharacterSpacing(this Gtk.Entry platformEntry, IEntry entry)
	{
		var instanceClass = CssCache.GetInstanceClass(platformEntry);
		var spacing = entry.CharacterSpacing;
		var spacingValue = $"{spacing}px";

		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ letter-spacing: {spacingValue}; }}");
		platformEntry.AddCssClass(instanceClass);
	}

	public static void UpdateCursorPosition(this Gtk.Entry platformEntry, IEntry entry)
	{
		platformEntry.SetPosition(entry.CursorPosition);
	}

	public static void UpdateSelectionLength(this Gtk.Entry platformEntry, IEntry entry)
	{
		if (entry.SelectionLength > 0)
		{
			var start = entry.CursorPosition;
			var end = start + entry.SelectionLength;
			platformEntry.SelectRegion(start, end);
		}
	}
}
