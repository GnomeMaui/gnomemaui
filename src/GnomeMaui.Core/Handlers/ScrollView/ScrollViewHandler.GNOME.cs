using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using System;

namespace Microsoft.Maui.Handlers;

public partial class ScrollViewHandler : ViewHandler<IScrollView, Gtk.ScrolledWindow>, ICrossPlatformLayout
{
	protected override Gtk.ScrolledWindow CreatePlatformView() => this.Create();

	public override void SetVirtualView(IView view)
	{
		base.SetVirtualView(view);
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set");

		if (PlatformView is ICrossPlatformLayoutBacking scrollView)
		{
			scrollView.CrossPlatformLayout = VirtualView;
		}
	}

	protected override void ConnectHandler(Gtk.ScrolledWindow platformView)
	{
		base.ConnectHandler(platformView);
		platformView.ConnectScrollHandler(this);
	}

	protected override void DisconnectHandler(Gtk.ScrolledWindow platformView)
	{
		platformView.DisconnectScrollHandler(this);
		base.DisconnectHandler(platformView);
	}

	public static void MapContent(IScrollViewHandler handler, IScrollView scrollView)
	{
		if (handler.PlatformView == null || handler.MauiContext == null)
			return;

		if (handler is not ICrossPlatformLayout)
			return;

		handler.UpdateContent(scrollView);
	}

	public static void MapHorizontalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
	{
		handler.PlatformView?.UpdateHorizontalScrollBarVisibility(scrollView);
	}

	public static void MapVerticalScrollBarVisibility(IScrollViewHandler handler, IScrollView scrollView)
	{
		handler.PlatformView?.UpdateVerticalScrollBarVisibility(scrollView);
	}

	public static void MapOrientation(IScrollViewHandler handler, IScrollView scrollView)
	{
		handler.PlatformView?.UpdateOrientation(scrollView);
	}

	public static void MapContentSize(IScrollViewHandler handler, IScrollView scrollView)
	{
		handler.PlatformView?.UpdateContentSize(scrollView);
	}

	public static void MapRequestScrollTo(IScrollViewHandler handler, IScrollView scrollView, object? args)
	{
		handler.PlatformView?.UpdateRequestScrollTo(scrollView, args);
	}

	Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
	{
		return VirtualView.CrossPlatformMeasure(widthConstraint, heightConstraint);
	}

	Size ICrossPlatformLayout.CrossPlatformArrange(Graphics.Rect bounds)
	{
		return VirtualView.CrossPlatformArrange(bounds);
	}
}
