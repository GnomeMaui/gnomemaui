using GnomeMaui.CSS;
using Microsoft.Maui.Graphics;
using System.Text;

namespace Microsoft.Maui.Platform;

public static class ButtonExtensions
{
	const string Prefix = "button";
	const string LabelPrefix = "label";

	public static Gtk.Button Create(this ButtonHandler _)
	{
		var button = Gtk.Button.NewWithLabel(string.Empty);
		button.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}");
		button.Child?.AddCssClass($"{CssCache.Prefix.TrimEnd('-')}{LabelPrefix}");
		return button;
	}

	private static string GetLabelInstanceClass(Gtk.Button platformButton)
	{
		// Csak a CSS osztály neve, Prefix nélkül
		return $"maui-{platformButton.Handle.DangerousGetHandle()}";
	}

	public static void UpdateText(this Gtk.Button platformButton, IText button)
	{
		platformButton.Label = button.Text ?? string.Empty;
	}

	public static void UpdateStrokeColor(this Gtk.Button platformButton, IButtonStroke buttonStroke)
	{
		if (buttonStroke.StrokeColor == null)
			return;

		var color = buttonStroke.StrokeColor;
		var instanceClass = CssCache.GetInstanceClass(platformButton);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ border-color: rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {color.Alpha}); }}");
		platformButton.AddCssClass(instanceClass);
	}

	public static void UpdateStrokeThickness(this Gtk.Button platformButton, IButtonStroke buttonStroke)
	{
		if (double.IsNaN(buttonStroke.StrokeThickness) || buttonStroke.StrokeThickness < 0)
			return;

		var instanceClass = CssCache.GetInstanceClass(platformButton);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ border-width: {buttonStroke.StrokeThickness}px; border-style: solid; }}");
		platformButton.AddCssClass(instanceClass);
	}

	public static void UpdateCornerRadius(this Gtk.Button platformButton, IButtonStroke buttonStroke)
	{
		var radius = buttonStroke.CornerRadius;
		if (radius < 0)
			return;

		var instanceClass = CssCache.GetInstanceClass(platformButton);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ border-radius: {radius}px; }}");
		platformButton.AddCssClass(instanceClass);
	}

	public static void UpdateTextColor(this Gtk.Button platformButton, ITextStyle button)
	{
		if (platformButton.Child is not Gtk.Label)
			return;

		if (button.TextColor is null)
			return;

		var color = button.TextColor;
		var instanceClass = GetLabelInstanceClass(platformButton);
		var instanceSelector = $"{LabelPrefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ color: rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {color.Alpha}); }}");
		platformButton.Child?.AddCssClass(instanceClass);
	}

	public static void UpdateCharacterSpacing(this Gtk.Button platformButton, ITextStyle button)
	{
		if (platformButton.Child is not Gtk.Label)
			return;

		var spacing = button.CharacterSpacing;
		if (spacing <= 0)
			return;

		var instanceClass = GetLabelInstanceClass(platformButton);
		var instanceSelector = $"{LabelPrefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ letter-spacing: {spacing}px; }}");
		platformButton.Child?.AddCssClass(instanceClass);
	}

	public static void UpdateFont(this Gtk.Button platformButton, ITextStyle button)
	{
		if (platformButton.Child is not Gtk.Label)
			return;

		var font = button.Font;

		if (font.IsDefault)
			return;

		var instanceClass = GetLabelInstanceClass(platformButton);
		var cssRules = new StringBuilder();

		if (!string.IsNullOrEmpty(font.Family))
		{
			cssRules.Append($"font-family: \"{font.Family}\"; ");
		}

		if (font.Size > 0)
		{
			cssRules.Append($"font-size: {font.Size}px; ");
		}

		if (font.Weight >= FontWeight.Bold)
		{
			cssRules.Append("font-weight: bold; ");
		}

		if (font.Slant == FontSlant.Italic)
		{
			cssRules.Append("font-style: italic; ");
		}

		if (cssRules.Length > 0)
		{
			var instanceSelector = $"{LabelPrefix}.{instanceClass}";
			CssCache.AddElementSelector($"{instanceSelector} {{ {cssRules} }}");
			platformButton.Child?.AddCssClass(instanceClass);
		}
	}

	public static void UpdatePadding(this Gtk.Button platformButton, IButton button)
	{
		if (button.Padding.IsNaN)
			return;

		var instanceClass = CssCache.GetInstanceClass(platformButton);
		var padding = button.Padding;
		var paddingCss = padding.Left == padding.Right && padding.Top == padding.Bottom
			? padding.Left == padding.Top
				? $"{padding.Top}px"
				: $"{padding.Top}px {padding.Left}px"
			: $"{padding.Top}px {padding.Right}px {padding.Bottom}px {padding.Left}px";

		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ padding: {paddingCss}; }}");
		platformButton.AddCssClass(instanceClass);
	}

	public static void UpdateBackground(this Gtk.Button platformButton, IButton button)
	{
		if (button.Background is not SolidPaint solidPaint)
			return;

		if (solidPaint.Color is null)
			return;

		var color = solidPaint.Color;
		var instanceClass = CssCache.GetInstanceClass(platformButton);
		var instanceSelector = $"{Prefix}.{instanceClass}";
		CssCache.AddElementSelector($"{instanceSelector} {{ background-color: rgba({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)}, {color.Alpha}); }}");
		platformButton.AddCssClass(instanceClass);
	}

	public static void UpdateContentLayout(this Gtk.Button platformButton, IButton button)
	{
		// ContentLayout handles image + text positioning
		// Since ImageSource is not yet implemented (MapImageSource is stub),
		// this will be expanded when image support is added
		// 
		// Future implementation will:
		// 1. Check if button has both ImageSource and Text
		// 2. Create Gtk.Box with orientation based on ContentLayout.Position:
		//    - Top/Bottom: Gtk.Orientation.Vertical
		//    - Left/Right: Gtk.Orientation.Horizontal
		// 3. Set Box.Spacing from ContentLayout.Spacing
		// 4. Append Image and Label to Box in correct order based on Position
		// 5. Set platformButton.Child = box
		//
		// For text-only buttons (current state), no action needed
		// as Gtk.Button.Label property is used directly
	}
}
