using GnomeMaui.CSS;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform;

public class ContentWidget : Adw.Bin//, ICrossPlatformLayoutBacking
{
	string _contentCssClass = string.Empty;
	public ContentWidget() : base()
	{
		var cssClass = $"contentwidget{Handle.DangerousGetHandle()}";
		AddCssClass(cssClass);
		CssCache.AddClassSelector($"{cssClass} {{ background-color: transparent; }}");
	}

	public ICrossPlatformLayout? CrossPlatformLayout { get; set; }

	Gtk.Widget? _content;

	public Gtk.Widget? Content
	{
		get => _content;
		set
		{
			SetChild(value);
			_content = value;
			_contentCssClass = $"contentwidget_content{Handle.DangerousGetHandle()}";
			_content?.AddCssClass(_contentCssClass);
			Console.WriteLine($"[ContentWidget] Setting Content: {_content?.GetType().Name}");
			_content?.Show();
		}
	}

	public void UpdatePadding(Microsoft.Maui.Thickness padding)
	{
		if (padding.IsNaN || _content == null)
			return;

		var top = padding.Top == 0 ? "0" : $"{padding.Top}px";
		var bottom = padding.Bottom == 0 ? "0" : $"{padding.Bottom}px";
		var left = padding.Left == 0 ? "0" : $"{padding.Left}px";
		var right = padding.Right == 0 ? "0" : $"{padding.Right}px";

		CssCache.AddClassSelector($"{_contentCssClass} {{ padding-top: {top}; padding-bottom: {bottom}; padding-left: {left}; padding-right: {right}; }}");
	}

	// Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
	// {
	// 	return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;
	// }

	// Size CrossPlatformArrange(Rect bounds)
	// {
	// 	return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Size.Zero;
	// }
}