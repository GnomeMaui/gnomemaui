using System;

namespace Microsoft.Maui.Handlers;

public partial class ContentViewHandler : ViewHandler<IContentView, ContentWidget>
{
	protected override ContentWidget CreatePlatformView()
	{
		if (VirtualView == null)
		{
			throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a ContentPanel");
		}

		var view = new ContentWidget
		{
			CrossPlatformLayout = VirtualView
		};

		return view;
	}

	public override void SetVirtualView(IView view)
	{
		base.SetVirtualView(view);
		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

		PlatformView.CrossPlatformLayout = VirtualView;
	}

	static void UpdateContent(IContentViewHandler handler)
	{
		_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
		_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

		// Add new content if exists
		if (handler.VirtualView.PresentedContent is IView view)
		{
			var platformContent = view.ToPlatform(handler.MauiContext);

			// If the widget already has a parent, unparent it (in case of reusing OLD widget)
			if (platformContent.GetParent() != null)
			{
				platformContent.Unparent();
			}

			handler.PlatformView.Content = platformContent;

			// Apply padding via CSS
			handler.PlatformView.UpdatePadding(handler.VirtualView.Padding);
		}
		else
		{
			// Remove previous content
			handler.PlatformView.Content = null;
		}
	}

	public static partial void MapContent(IContentViewHandler handler, IContentView page)
	{
		UpdateContent(handler);
	}

	public static partial void MapPadding(IContentViewHandler handler, IContentView page)
	{
		handler.PlatformView?.UpdatePadding(handler.VirtualView.Padding);
	}

	protected override void DisconnectHandler(ContentWidget platformView)
	{
		// Critical: Unparent the child before the handler is disconnected
		if (platformView.Content != null)
		{
			platformView.Content.Unparent();
			platformView.Content = null;
		}

		base.DisconnectHandler(platformView);
	}
}
