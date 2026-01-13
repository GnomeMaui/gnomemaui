using GnomeMaui.CSS;
using System.Globalization;

namespace Microsoft.Maui.Platform;

public static class LabelExtensions
{
	private const string Prefix = "label";

	public static Gtk.Label Create(this LabelHandler _)
	{
		var label = new Gtk.Label
		{
			Wrap = false,
			WrapMode = Pango.WrapMode.None
		};
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
		var attrList = platformLabel.GetAttributes() ?? Pango.AttrList.New();
		var text = label.Text ?? string.Empty;
		int textLength = System.Text.Encoding.UTF8.GetByteCount(text);
		if (label.LineHeight >= 0 && label.LineHeight != 1.0)
		{
			var lineSpacingAttr = Pango.Functions.AttrLineHeightNew(label.LineHeight);
			lineSpacingAttr.StartIndex = 0;
			lineSpacingAttr.EndIndex = (uint)textLength;
			attrList.Insert(lineSpacingAttr);
		}
		platformLabel.SetAttributes(attrList);
		platformLabel.QueueResize();
		platformLabel.QueueDraw();

		// var instanceClass = CssCache.GetInstanceClass(platformLabel);
		// var lineHeight = label.LineHeight;
		// if (lineHeight >= 0)
		// {
		// 	var instanceSelector = $"{Prefix}.{instanceClass}";
		// 	CssCache.AddElementSelector($"{instanceSelector} {{ line-height: {lineHeight.ToString(CultureInfo.InvariantCulture)}; }}");
		// 	platformLabel.AddCssClass(instanceClass);
		// }
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


// using GnomeMaui.CSS;

// namespace Microsoft.Maui.Platform;

// public static class LabelExtensions
// {
// 	private const string Prefix = "label";

// 	public static Gtk.Label Create(this LabelHandler _)
// 	{
// 		var label = new Gtk.Label
// 		{
// 			WrapMode = Pango.WrapMode.None
// 		};
// 		label.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
// 		return label;
// 	}

// 	public static void UpdateText(this Gtk.Label platformLabel, ILabel label)
// 	{
// 		platformLabel.Label_ = label.Text ?? string.Empty;
// 		RefreshAttributes(platformLabel, label);
// 		label.InvalidateMeasure();
// 	}

// 	public static void UpdateTextColor(this Gtk.Label platformLabel, ILabel label)
// 	{
// 		RefreshAttributes(platformLabel, label);
// 		label.InvalidateMeasure();
// 	}

// 	public static void UpdateCharacterSpacing(this Gtk.Label platformLabel, ILabel label)
// 	{
// 		RefreshAttributes(platformLabel, label);
// 		label.InvalidateMeasure();
// 	}

// 	public static void UpdateFont(this Gtk.Label platformLabel, ILabel label)
// 	{
// 		RefreshAttributes(platformLabel, label);
// 		label.InvalidateMeasure();
// 	}

// 	public static void UpdateTextDecorations(this Gtk.Label platformLabel, ILabel label)
// 	{
// 		RefreshAttributes(platformLabel, label);
// 		label.InvalidateMeasure();
// 	}

// 	public static void UpdateLineHeight(this Gtk.Label platformLabel, ILabel label)
// 	{
// 		RefreshAttributes(platformLabel, label);
// 		label.InvalidateMeasure();
// 	}

// 	private static void RefreshAttributes(Gtk.Label platformLabel, ILabel label)
// 	{
// 		var attrList = Pango.AttrList.New();
// 		var text = label.Text ?? string.Empty;
// 		int textLength = System.Text.Encoding.UTF8.GetByteCount(text);

// 		// 1. TEXT COLOR
// 		if (label.TextColor != null)
// 		{
// 			var color = label.TextColor;
// 			var r = (ushort)(color.Red * 65535);
// 			var g = (ushort)(color.Green * 65535);
// 			var b = (ushort)(color.Blue * 65535);

// 			var colorAttr = Pango.Functions.AttrForegroundNew(r, g, b);
// 			colorAttr.StartIndex = 0;
// 			colorAttr.EndIndex = (uint)textLength;
// 			attrList.Insert(colorAttr);

// 			// Alpha
// 			if (color.Alpha < 1.0f)
// 			{
// 				ushort alpha = (ushort)(color.Alpha * 65535);
// 				var alphaAttr = Pango.Functions.AttrForegroundAlphaNew(alpha);
// 				alphaAttr.StartIndex = 0;
// 				alphaAttr.EndIndex = (uint)textLength;
// 				attrList.Insert(alphaAttr);
// 			}
// 		}

// 		// 2. CHARACTER SPACING (letter-spacing)
// 		if (label.CharacterSpacing != 0)
// 		{
// 			int spacing = (int)(label.CharacterSpacing * 1024);
// 			var spacingAttr = Pango.Functions.AttrLetterSpacingNew(spacing);
// 			spacingAttr.StartIndex = 0;
// 			spacingAttr.EndIndex = (uint)textLength;
// 			attrList.Insert(spacingAttr);
// 		}

// 		// 3. FONT FAMILY
// 		if (!string.IsNullOrEmpty(label.Font.Family))
// 		{
// 			var familyAttr = Pango.Functions.AttrFamilyNew(label.Font.Family);
// 			familyAttr.StartIndex = 0;
// 			familyAttr.EndIndex = (uint)textLength;
// 			attrList.Insert(familyAttr);
// 		}

// 		// 4. FONT SIZE
// 		if (label.Font.Size > 0)
// 		{
// 			// MAUI FontSize in pixels, but Pango expects size in points
// 			// 1 point = 96/72 pixel = 1.333... pixel
// 			// Conversion: points = pixels * 72 / 96
// 			double fontSizeInPoints = label.Font.Size * 72.0 / 96.0; // pixel to points
// 			int sizeInPangoUnits = (int)(fontSizeInPoints * Pango.Constants.SCALE);
// 			var sizeAttr = Pango.Functions.AttrSizeNew(sizeInPangoUnits);
// 			sizeAttr.StartIndex = 0;
// 			sizeAttr.EndIndex = (uint)textLength;
// 			attrList.Insert(sizeAttr);
// 		}

// 		// 5. FONT WEIGHT (Bold)
// 		if (label.Font.Weight >= FontWeight.Bold)
// 		{
// 			var weightAttr = Pango.Functions.AttrWeightNew(Pango.Weight.Bold);
// 			weightAttr.StartIndex = 0;
// 			weightAttr.EndIndex = (uint)textLength;
// 			attrList.Insert(weightAttr);
// 		}

// 		// 6. FONT STYLE (Italic)
// 		if (label.Font.Slant == FontSlant.Italic)
// 		{
// 			var styleAttr = Pango.Functions.AttrStyleNew(Pango.Style.Italic);
// 			styleAttr.StartIndex = 0;
// 			styleAttr.EndIndex = (uint)textLength;
// 			attrList.Insert(styleAttr);
// 		}

// 		// 7. TEXT DECORATIONS
// 		if (label.TextDecorations != TextDecorations.None)
// 		{
// 			if (label.TextDecorations.HasFlag(TextDecorations.Underline))
// 			{
// 				var underlineAttr = Pango.Functions.AttrUnderlineNew(Pango.Underline.Single);
// 				underlineAttr.StartIndex = 0;
// 				underlineAttr.EndIndex = (uint)textLength;
// 				attrList.Insert(underlineAttr);
// 			}

// 			if (label.TextDecorations.HasFlag(TextDecorations.Strikethrough))
// 			{
// 				var strikeAttr = Pango.Functions.AttrStrikethroughNew(true);
// 				strikeAttr.StartIndex = 0;
// 				strikeAttr.EndIndex = (uint)textLength;
// 				attrList.Insert(strikeAttr);
// 			}
// 		}

// 		// 8. LINE HEIGHT (line spacing)
// 		if (label.LineHeight >= 0 && label.LineHeight != 1.0)
// 		{
// 			var lineSpacingAttr = Pango.Functions.AttrLineHeightNew(label.LineHeight);
// 			lineSpacingAttr.StartIndex = 0;
// 			lineSpacingAttr.EndIndex = (uint)textLength;
// 			attrList.Insert(lineSpacingAttr);
// 		}

// 		// Apply Attributes
// 		platformLabel.SetAttributes(attrList);
// 	}

// 	// 9. TEXT ALIGNMENTS
// 	public static void UpdateHorizontalTextAlignment(this Gtk.Label platformLabel, ILabel label)
// 	{
// 		var justification = label.HorizontalTextAlignment switch
// 		{
// 			TextAlignment.Start => Gtk.Justification.Left,
// 			TextAlignment.Center => Gtk.Justification.Center,
// 			TextAlignment.End => Gtk.Justification.Right,
// 			_ => Gtk.Justification.Left
// 		};
// 		platformLabel.Justify = justification;

// 		var xalign = label.HorizontalTextAlignment switch
// 		{
// 			TextAlignment.Start => 0.0f,
// 			TextAlignment.Center => 0.5f,
// 			TextAlignment.End => 1.0f,
// 			_ => 0.0f
// 		};
// 		platformLabel.Xalign = xalign;
// 		label.InvalidateMeasure();
// 	}

// 	public static void UpdateVerticalTextAlignment(this Gtk.Label platformLabel, ILabel label)
// 	{
// 		var yalign = label.VerticalTextAlignment switch
// 		{
// 			TextAlignment.Start => 0.0f,
// 			TextAlignment.Center => 0.5f,
// 			TextAlignment.End => 1.0f,
// 			_ => 0.0f
// 		};
// 		platformLabel.Yalign = yalign;
// 		label.InvalidateMeasure();
// 	}

// 	public static void UpdatePadding(this Gtk.Label platformLabel, ILabel label)
// 	{
// 		var instanceClass = CssCache.GetInstanceClass(platformLabel);
// 		var padding = label.Padding;
// 		platformLabel.RemoveManagedData(nameof(label.Padding));
// 		platformLabel.SetManagedData(nameof(label.Padding), (object)padding);

// 		var paddingCss = padding.Left == padding.Right && padding.Top == padding.Bottom
// 			? padding.Left == padding.Top
// 				? $"{padding.Top}px"
// 				: $"{padding.Top}px {padding.Left}px"
// 			: $"{padding.Top}px {padding.Right}px {padding.Bottom}px {padding.Left}px";

// 		var instanceSelector = $"{Prefix}.{instanceClass}";
// 		CssCache.AddElementSelector($"{instanceSelector} {{ padding: {paddingCss}; }}");
// 		platformLabel.AddCssClass(instanceClass);
// 		label.InvalidateMeasure();
// 	}
// }


