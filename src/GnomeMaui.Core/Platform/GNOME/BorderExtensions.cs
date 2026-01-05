using System;

namespace Microsoft.Maui.Platform;

public static class BorderExtensions
{
	public static ContentWidget Create(this IBorderHandler handler)
	{
		_ = handler.VirtualView ?? throw new InvalidOperationException($"VirtualView must be set to create a ContentWidget");

		var view = new ContentWidget
		{
			CrossPlatformLayout = handler.VirtualView
		};

		return view;
	}

	public static void UpdateContent(this IBorderHandler handler)
	{
		_ = handler.PlatformView ?? throw new InvalidOperationException($"PlatformView should have been set by base class.");
		_ = handler.VirtualView ?? throw new InvalidOperationException($"VirtualView should have been set by base class.");
		_ = handler.MauiContext ?? throw new InvalidOperationException($"MauiContext should have been set by base class.");

		// Remove previous content
		handler.PlatformView.Content = null;

		// Add new content if exists
		if (handler.VirtualView.PresentedContent is IView view)
		{
			handler.PlatformView.Content = view.ToPlatform(handler.MauiContext);

			// Apply padding via CSS
			handler.PlatformView.UpdatePadding(handler.VirtualView.Padding);
		}
	}

	public static void UpdateBackground(this ContentWidget platformView, IBorderView border)
	{
		if (platformView.GetParent() is WrapperView wrapperView)
		{
			wrapperView.UpdateBackground(border.Background);
		}
	}
}