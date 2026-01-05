using GnomeMaui.CSS;

namespace Microsoft.Maui.Platform;

public static class LabelExtensions
{
	private const string Prefix = "label";

	public static Gtk.Label Create(this LabelHandler _)
	{
		var label = new Gtk.Label();
		label.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
		return label;
	}

	public static void UpdateText(this Gtk.Label platformLabel, ILabel label)
	{
		platformLabel.Label_ = label.Text ?? string.Empty;
	}

	public static void UpdateTextColor(this Gtk.Label platformLabel, ILabel label)
	{
		if (label.TextColor == null)
		{
			return;
		}

		var color = label.TextColor;
		var (r, g, b, a) = ((int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255), color.Alpha);
		var colorValue = $"rgba({r}, {g}, {b}, {a})";

		var instanceClass = CssCache.GetInstanceClass(platformLabel);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ color: {colorValue}; }}");

		platformLabel?.AddCssClass(instanceClass);
	}

	public static void UpdateCharacterSpacing(this Gtk.Label platformLabel, ILabel label)
	{
		var instanceClass = CssCache.GetInstanceClass(platformLabel);
		var spacing = label.CharacterSpacing;
		var spacingValue = $"{spacing}px";

		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ letter-spacing: {spacingValue}; }}");
		platformLabel.AddCssClass(instanceClass);
	}

	public static void UpdateFont(this Gtk.Label platformLabel, ILabel label)
	{
		var instanceClass = CssCache.GetInstanceClass(platformLabel);
		var cssRules = new System.Text.StringBuilder();

		// Font family
		if (!string.IsNullOrEmpty(label.Font.Family))
		{
			var fontFamilyValue = $"\"{label.Font.Family}\"";
			cssRules.Append($"font-family: {fontFamilyValue}; ");
		}

		// Font size
		var size = label.Font.Size;
		if (size > 0)
		{
			var fontSizeValue = $"{size}px";
			cssRules.Append($"font-size: {fontSizeValue}; ");
		}

		// Font weight (Bold)
		if (label.Font.Weight >= FontWeight.Bold)
		{
			cssRules.Append("font-weight: bold; ");
		}

		// Font style (Italic)
		if (label.Font.Slant == FontSlant.Italic)
		{
			cssRules.Append("font-style: italic; ");
		}

		if (cssRules.Length > 0)
		{
			var instanceSelector = $"{Prefix}.{instanceClass}";
			CssCache.AddElementSelector($"{instanceSelector} {{ {cssRules} }}");
			platformLabel.AddCssClass(instanceClass);
		}
	}

	public static void UpdateHorizontalTextAlignment(this Gtk.Label platformLabel, ILabel label)
	{
		// GTK natív Justify property használata CSS helyett
		var justification = label.HorizontalTextAlignment switch
		{
			TextAlignment.Start => Gtk.Justification.Left,
			TextAlignment.Center => Gtk.Justification.Center,
			TextAlignment.End => Gtk.Justification.Right,
			_ => Gtk.Justification.Left
		};
		platformLabel.Justify = justification;

		// Xalign is needed for single-line labels
		var xalign = label.HorizontalTextAlignment switch
		{
			TextAlignment.Start => 0.0f,
			TextAlignment.Center => 0.5f,
			TextAlignment.End => 1.0f,
			_ => 0.0f
		};
		platformLabel.Xalign = xalign;
	}

	public static void UpdateVerticalTextAlignment(this Gtk.Label platformLabel, ILabel label)
	{
		// GTK natív Yalign property használata CSS helyett
		var yalign = label.VerticalTextAlignment switch
		{
			TextAlignment.Start => 0.0f,
			TextAlignment.Center => 0.5f,
			TextAlignment.End => 1.0f,
			_ => 0.0f
		};
		platformLabel.Yalign = yalign;
	}

	public static void UpdateLineHeight(this Gtk.Label platformLabel, ILabel label)
	{
		var instanceClass = CssCache.GetInstanceClass(platformLabel);
		var lineHeight = label.LineHeight;
		if (lineHeight >= 0)
		{
			var instanceSelector = $"{Prefix}.{instanceClass}";
			CssCache.AddElementSelector($"{instanceSelector} {{ line-height: {lineHeight}; }}");
			platformLabel.AddCssClass(instanceClass);
		}
	}

	public static void UpdatePadding(this Gtk.Label platformLabel, ILabel label)
	{
		var instanceClass = CssCache.GetInstanceClass(platformLabel);
		var padding = label.Padding;
		var paddingCss = padding.Left == padding.Right && padding.Top == padding.Bottom
			? padding.Left == padding.Top
				? $"{padding.Top}px"
				: $"{padding.Top}px {padding.Left}px"
			: $"{padding.Top}px {padding.Right}px {padding.Bottom}px {padding.Left}px";

		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ padding: {paddingCss}; }}");
		platformLabel.AddCssClass(instanceClass);
	}

	public static void UpdateTextDecorations(this Gtk.Label platformLabel, ILabel label)
	{
		var instanceClass = CssCache.GetInstanceClass(platformLabel);
		var decorations = label.TextDecorations;
		var decorationCss = "none";

		if (decorations != TextDecorations.None)
		{
			var parts = new System.Collections.Generic.List<string>();
			if (decorations.HasFlag(TextDecorations.Underline))
				parts.Add("underline");
			if (decorations.HasFlag(TextDecorations.Strikethrough))
				parts.Add("line-through");

			if (parts.Count > 0)
				decorationCss = string.Join(" ", parts);
		}

		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ text-decoration: {decorationCss}; }}");
		platformLabel.AddCssClass(instanceClass);
	}
}
