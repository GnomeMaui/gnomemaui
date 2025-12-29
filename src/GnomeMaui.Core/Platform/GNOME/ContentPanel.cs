using Gtk;
using Microsoft.Maui.Graphics;
using System;

namespace Microsoft.Maui.Platform;

public class ContentPanel : Adw.Bin, ICrossPlatformLayoutBacking
{
	public ContentPanel() : base() { }

	public ICrossPlatformLayout? CrossPlatformLayout { get; set; }

	Widget? _content;

	public Widget? Content
	{
		get => _content;
		set
		{
			Child = value;
			_content = value;
		}
	}

	Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
	{
		return CrossPlatformLayout?.CrossPlatformMeasure(widthConstraint, heightConstraint) ?? Size.Zero;
	}

	Size CrossPlatformArrange(Rect bounds)
	{
		return CrossPlatformLayout?.CrossPlatformArrange(bounds) ?? Size.Zero;
	}
}