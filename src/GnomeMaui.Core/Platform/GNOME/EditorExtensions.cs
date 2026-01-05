using GnomeMaui.CSS;

namespace Microsoft.Maui.Platform;

public static class EditorExtensions
{
	private const string Prefix = "textview";

	public static Gtk.TextView Create(this EditorHandler _)
	{
		var textView = new Gtk.TextView();
		textView.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
		textView.WrapMode = Gtk.WrapMode.Word;

		// Add rounded corners like GtkEntry (border-radius: 5px)
		var instanceClass = CssCache.GetInstanceClass(textView);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		// TODO: Make radius and padding configurable
		CssCache.AddElementSelector($"{instanceSelector} {{ border-radius: 8px; padding: 6px; }}");
		textView.AddCssClass(instanceClass);

		return textView;
	}

	public static void UpdateText(this Gtk.TextView platformEditor, IEditor editor)
	{
		var buffer = platformEditor.GetBuffer();
		if (buffer != null)
		{
			buffer.SetText(editor.Text ?? string.Empty, -1);
		}
	}

	public static void UpdateTextColor(this Gtk.TextView platformEditor, IEditor editor)
	{
		if (editor.TextColor == null)
		{
			return;
		}

		var color = editor.TextColor;
		var (r, g, b, a) = ((int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255), color.Alpha);
		var colorValue = $"rgba({r}, {g}, {b}, {a})";

		var instanceClass = CssCache.GetInstanceClass(platformEditor);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ color: {colorValue}; }}");

		platformEditor?.AddCssClass(instanceClass);
	}

	public static void UpdatePlaceholder(this Gtk.TextView platformEditor, IEditor editor)
	{
		// GTK TextView doesn't have built-in placeholder support
		// This would require custom rendering or overlay widget
	}

	public static void UpdatePlaceholderColor(this Gtk.TextView platformEditor, IEditor editor)
	{
		// GTK TextView doesn't have built-in placeholder support
	}

	public static void UpdateCharacterSpacing(this Gtk.TextView platformEditor, IEditor editor)
	{
		var instanceClass = CssCache.GetInstanceClass(platformEditor);
		var spacing = editor.CharacterSpacing;
		var spacingValue = $"{spacing}px";

		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ letter-spacing: {spacingValue}; }}");
		platformEditor.AddCssClass(instanceClass);
	}

	public static void UpdateMaxLength(this Gtk.TextView platformEditor, IEditor editor)
	{
		// GTK TextView doesn't have direct max length property
		// This would require buffer signal handling
	}

	public static void UpdateIsTextPredictionEnabled(this Gtk.TextView platformEditor, IEditor editor)
	{
		platformEditor.InputPurpose = editor.IsTextPredictionEnabled
			? Gtk.InputPurpose.FreeForm
			: Gtk.InputPurpose.Terminal;
	}

	public static void UpdateIsSpellCheckEnabled(this Gtk.TextView platformEditor, IEditor editor)
	{
		// GTK TextView doesn't have built-in spell check property in GirCore
		// This would require GtkSpell or similar
	}

	public static void UpdateFont(this Gtk.TextView platformEditor, IEditor editor)
	{
		var instanceClass = CssCache.GetInstanceClass(platformEditor);
		var cssRules = new System.Text.StringBuilder();

		// Font family
		if (!string.IsNullOrEmpty(editor.Font.Family))
		{
			var fontFamilyValue = $"\"{editor.Font.Family}\"";
			cssRules.Append($"font-family: {fontFamilyValue}; ");
		}

		// Font size
		var size = editor.Font.Size;
		if (size > 0)
		{
			var fontSizeValue = $"{size}px";
			cssRules.Append($"font-size: {fontSizeValue}; ");
		}

		// Font weight (Bold)
		if (editor.Font.Weight >= FontWeight.Bold)
		{
			cssRules.Append("font-weight: bold; ");
		}

		// Font style (Italic)
		if (editor.Font.Slant == FontSlant.Italic)
		{
			cssRules.Append("font-style: italic; ");
		}

		if (cssRules.Length > 0)
		{
			var instanceSelector = $"{Prefix}.{instanceClass}";
			CssCache.AddElementSelector($"{instanceSelector} {{ {cssRules} }}");
			platformEditor.AddCssClass(instanceClass);
		}
	}

	public static void UpdateIsReadOnly(this Gtk.TextView platformEditor, IEditor editor)
	{
		platformEditor.Editable = !editor.IsReadOnly;
	}

	public static void UpdateHorizontalTextAlignment(this Gtk.TextView platformEditor, IEditor editor)
	{
		var justification = editor.HorizontalTextAlignment switch
		{
			TextAlignment.Start => Gtk.Justification.Left,
			TextAlignment.Center => Gtk.Justification.Center,
			TextAlignment.End => Gtk.Justification.Right,
			_ => Gtk.Justification.Left
		};
		platformEditor.Justification = justification;
	}

	public static void UpdateVerticalTextAlignment(this Gtk.TextView platformEditor, IEditor editor)
	{
		// GTK TextView doesn't have direct vertical alignment
		// This would need container alignment
	}

	public static void UpdateKeyboard(this Gtk.TextView platformEditor, IEditor editor)
	{
		var keyboard = editor.Keyboard;

		platformEditor.InputPurpose = keyboard switch
		{
			Keyboard k when k == Keyboard.Email => Gtk.InputPurpose.Email,
			Keyboard k when k == Keyboard.Numeric => Gtk.InputPurpose.Number,
			Keyboard k when k == Keyboard.Telephone => Gtk.InputPurpose.Phone,
			Keyboard k when k == Keyboard.Url => Gtk.InputPurpose.Url,
			_ => Gtk.InputPurpose.FreeForm
		};
	}

	public static void UpdateCursorPosition(this Gtk.TextView platformEditor, ITextInput editor)
	{
		var buffer = platformEditor.GetBuffer();
		if (buffer != null)
		{
			buffer.GetIterAtOffset(out var iter, editor.CursorPosition);
			buffer.PlaceCursor(iter);
		}
	}

	public static void UpdateSelectionLength(this Gtk.TextView platformEditor, ITextInput editor)
	{
		var buffer = platformEditor.GetBuffer();
		if (buffer != null && editor.SelectionLength > 0)
		{
			buffer.GetIterAtOffset(out var startIter, editor.CursorPosition);
			buffer.GetIterAtOffset(out var endIter, editor.CursorPosition + editor.SelectionLength);
			buffer.SelectRange(startIter, endIter);
		}
	}
}
