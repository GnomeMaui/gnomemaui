using GnomeMaui.CSS;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform;

public class ContentWidget : Adw.Bin
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

	private string GetWidgetInfo(Gtk.Widget? widget)
	{
		if (widget == null)
			return "null";

		var typeName = widget.GetType().Name;
		var handle = $"0x{widget.Handle.DangerousGetHandle():x}";

		// If it's a LayoutWidget, include the MAUI element type
		if (widget is LayoutWidget layoutWidget && layoutWidget.CrossPlatformLayout != null)
		{
			var mauiType = layoutWidget.CrossPlatformLayout.GetType().Name;
			return $"{typeName} (MAUI: {mauiType}, Handle: {handle})";
		}

		return $"{typeName} (Handle: {handle})";
	}

	private void PrintWidgetParentHierarchy(Gtk.Widget? widget, string prefix = "", int depth = 0)
	{
		if (widget == null)
		{
			return;
		}

		if (depth > 20)
		{
			return;
		}

		var parent = widget.GetParent();
		if (parent != null)
		{
			PrintWidgetParentHierarchy(parent, prefix + "  ", depth + 1);
		}
	}

	public Gtk.Widget? Content
	{
		get => _content;
		set
		{
			// Validate the new value first BEFORE removing existing child
			if (value != null && value.Handle.DangerousGetHandle() == IntPtr.Zero)
			{
				_content = null;
				return;
			}

			// Remove existing child first to avoid "widget already has parent" error
			var existingChild = GetChild();
			if (existingChild != null)
			{
				PrintWidgetParentHierarchy(existingChild, "  ");

				// Validate existing child is still valid
				if (existingChild.Handle.DangerousGetHandle() == IntPtr.Zero)
				{
					// Clear it without calling Unparent
				}
				else
				{
					// Check if it's the SAME widget by comparing native handles
					bool isSameWidget = value != null && existingChild.Handle.DangerousGetHandle() == value.Handle.DangerousGetHandle();

					if (isSameWidget)
					{
						// Same widget, no need to do anything
						_content = value;
						return;
					}

					// Different widget, validate it still has a parent before Unparent
					var parent = existingChild.GetParent();
					if (parent == null)
					{
						// Widget already unparented or disposed at GTK level
					}
					else
					{
						// Valid widget with parent, unparent it
						existingChild.Unparent();
					}
				}
			}

			// Set new child
			if (value != null)
			{
				PrintWidgetParentHierarchy(value, "  ");
				// CRITICAL CHECK: Ha a widget már MÁS parent-ben van, akkor cache probléma!
				var widgetParent = value.GetParent();
				if (widgetParent != null)
				{
					// Check if it's the SAME parent by comparing native handles
					bool isSameParent = widgetParent.Handle.DangerousGetHandle() == this.Handle.DangerousGetHandle();

					if (!isSameParent)
					{
						_content = null;
						return;
					}
				}
			}

			SetChild(value);
			_content = value;
			_contentCssClass = $"contentwidget_content{Handle.DangerousGetHandle()}";
			_content?.AddCssClass(_contentCssClass);
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
}