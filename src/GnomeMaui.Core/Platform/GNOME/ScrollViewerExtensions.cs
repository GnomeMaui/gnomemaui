using Microsoft.Maui.Graphics;
using System;

namespace Microsoft.Maui.Platform;

public static class ScrollViewerExtensions
{
	public static Gtk.ScrolledWindow Create(this ScrollViewHandler _)
	{
		var scrolledWindow = Gtk.ScrolledWindow.New();
		scrolledWindow.SetHexpand(true);
		scrolledWindow.SetVexpand(true);

		// FONTOS: Ezeket NEM kell beállítani, mert akkor a ScrolledWindow 
		// maga próbál akkora lenni mint a tartalom, ami megakadályozza a scrollt
		// scrolledWindow.SetPropagateNaturalHeight(true);
		// scrolledWindow.SetPropagateNaturalWidth(true);

		return scrolledWindow;
	}

	public static void UpdateContent(this IScrollViewHandler handler, IScrollView scrollView)
	{
		if (handler.PlatformView == null || handler.MauiContext == null)
			return;

		var platformView = handler.PlatformView;

		// Remove existing child
		var existingChild = platformView.GetChild();
		if (existingChild != null)
		{
			platformView.SetChild(null);
		}

		// Add new content if exists
		if (scrollView.PresentedContent is IView content)
		{
			var nativeContent = content.ToPlatform(handler.MauiContext);

			// The content should expand horizontally to use the ScrollView's width
			// for proper wrapping layouts like FlexLayout. Vertical expansion is
			// disabled to allow vertical scrolling when content exceeds viewport.
			if (nativeContent != null)
			{
				nativeContent.SetHexpand(true);
				nativeContent.SetVexpand(false);
				nativeContent.Show();
			}

			platformView.SetChild(nativeContent);
		}
	}

	public static void UpdateVerticalScrollBarVisibility(this Gtk.ScrolledWindow scrollView, IScrollView virtualView)
	{
		var policy = virtualView.VerticalScrollBarVisibility switch
		{
			ScrollBarVisibility.Always => Gtk.PolicyType.Always,
			ScrollBarVisibility.Never => Gtk.PolicyType.Never,
			ScrollBarVisibility.Default => Gtk.PolicyType.Automatic,
			_ => Gtk.PolicyType.Automatic
		};

		scrollView.SetPolicy(scrollView.HscrollbarPolicy, policy);
	}

	public static void UpdateHorizontalScrollBarVisibility(this Gtk.ScrolledWindow scrollView, IScrollView virtualView)
	{
		var policy = virtualView.HorizontalScrollBarVisibility switch
		{
			ScrollBarVisibility.Always => Gtk.PolicyType.Always,
			ScrollBarVisibility.Never => Gtk.PolicyType.Never,
			ScrollBarVisibility.Default => Gtk.PolicyType.Automatic,
			_ => Gtk.PolicyType.Automatic
		};

		scrollView.SetPolicy(policy, scrollView.VscrollbarPolicy);
	}

	public static void UpdateOrientation(this Gtk.ScrolledWindow scrollView, IScrollView virtualView)
	{
		switch (virtualView.Orientation)
		{
			case ScrollOrientation.Horizontal:
				scrollView.SetPolicy(Gtk.PolicyType.Automatic, Gtk.PolicyType.Never);
				break;
			case ScrollOrientation.Vertical:
				scrollView.SetPolicy(Gtk.PolicyType.Never, Gtk.PolicyType.Automatic);
				break;
			case ScrollOrientation.Both:
				scrollView.SetPolicy(Gtk.PolicyType.Automatic, Gtk.PolicyType.Automatic);
				break;
			case ScrollOrientation.Neither:
				scrollView.SetPolicy(Gtk.PolicyType.Never, Gtk.PolicyType.Never);
				break;
		}
	}

	public static void UpdateContentSize(this Gtk.ScrolledWindow scrollView, IScrollView virtualView)
	{
		if (virtualView.ContentSize.Width > 0)
			scrollView.SetMinContentWidth((int)virtualView.ContentSize.Width);

		if (virtualView.ContentSize.Height > 0)
			scrollView.SetMinContentHeight((int)virtualView.ContentSize.Height);
	}

	public static void UpdateRequestScrollTo(this Gtk.ScrolledWindow scrollView, IScrollView virtualView, object? args)
	{
		if (args is not ScrollToRequest request)
			return;

		var hadj = scrollView.GetHadjustment();
		var vadj = scrollView.GetVadjustment();

		if (hadj != null)
		{
			var maxH = hadj.GetUpper() - hadj.GetPageSize();
			var targetH = Math.Min(request.HorizontalOffset, maxH);
			hadj.SetValue(targetH);
		}

		if (vadj != null)
		{
			var maxV = vadj.GetUpper() - vadj.GetPageSize();
			var targetV = Math.Min(request.VerticalOffset, maxV);
			vadj.SetValue(targetV);
		}
	}

	public static void ConnectScrollHandler(this Gtk.ScrolledWindow platformView, IScrollViewHandler handler)
	{
		platformView.GetHadjustment()?.OnValueChanged += (sender, args) => OnScrolled(platformView, handler);
		platformView.GetVadjustment()?.OnValueChanged += (sender, args) => OnScrolled(platformView, handler);
	}

	public static void DisconnectScrollHandler(this Gtk.ScrolledWindow platformView, IScrollViewHandler handler)
	{
		if (platformView.GetHadjustment() is { } hadj)
			hadj.OnValueChanged -= (sender, args) => OnScrolled(platformView, handler);

		if (platformView.GetVadjustment() is { } vadj)
			vadj.OnValueChanged -= (sender, args) => OnScrolled(platformView, handler);

		if (platformView is ICrossPlatformLayoutBacking scrollView)
			scrollView.CrossPlatformLayout = null;
	}

	static void OnScrolled(Gtk.ScrolledWindow platformView, IScrollViewHandler handler)
	{
		if (handler.VirtualView == null)
			return;

		var hadj = platformView.GetHadjustment();
		var vadj = platformView.GetVadjustment();

		if (hadj != null)
			handler.VirtualView.HorizontalOffset = hadj.GetValue();

		if (vadj != null)
			handler.VirtualView.VerticalOffset = vadj.GetValue();

		handler.VirtualView.ScrollFinished();
	}
}