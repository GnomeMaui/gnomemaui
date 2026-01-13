using Adw;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;

namespace Microsoft.Maui.Handlers;

// Window handler for GirCore/Adwaita that provides a platform Adw.ApplicationWindow for a MAUI Window
public partial class WindowHandler : ElementHandler<IWindow, Adw.ApplicationWindow>
{
	NavigationRootManager? _navigationRootManager;

	protected override void ConnectHandler(Adw.ApplicationWindow platformView)
	{
		base.ConnectHandler(platformView);

		// Subscribe to size changes to update MAUI Window.Width/Height for AdaptiveTrigger
		platformView.OnNotify += OnPlatformViewPropertyChanged;

		// Create NavigationRootManager for managing WindowRootView with HeaderBar
		if (MauiContext != null)
		{
			_navigationRootManager = MauiContext.GetNavigationRootManager();
		}
	}

	protected override void DisconnectHandler(Adw.ApplicationWindow platformView)
	{
		platformView.OnNotify -= OnPlatformViewPropertyChanged;
		_navigationRootManager?.Disconnect();
		_navigationRootManager = null;
		base.DisconnectHandler(platformView);
	}

	void OnPlatformViewPropertyChanged(GObject.Object sender, GObject.Object.NotifySignalArgs args)
	{
		var propName = args.Pspec.GetName();

		// Update MAUI Window.Width/Height when GTK window size changes
		// This is needed for AdaptiveTrigger to work
		if ((propName == "default-width" || propName == "default-height") && VirtualView is IWindow window)
		{
			// Use GetWidth/GetHeight for immediate values during resize
			var newWidth = PlatformView.GetWidth();
			var newHeight = PlatformView.GetHeight();

			if (newWidth > 0 && newHeight > 0)
			{
				// Use FrameChanged to update window size (Width/Height are read-only)
				window.FrameChanged(new Graphics.Rect(0, 0, newWidth, newHeight));
			}
		}
	}

	public static void MapTitle(IWindowHandler handler, IWindow window)
	{
		if (handler is WindowHandler gnomeHandler && gnomeHandler._navigationRootManager != null)
		{
			gnomeHandler._navigationRootManager.WindowTitle = window.Title ?? string.Empty;
		}
		else if (handler.PlatformView is Adw.ApplicationWindow appWindow)
		{
			appWindow.Title = window.Title ?? string.Empty;
		}
	}

	public static void MapContent(IWindowHandler handler, IWindow window)
	{
		_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

		var content = window.Content as IView;
		if (content != null && content.Handler == null && handler.MauiContext != null)
		{
			_ = content.ToHandler(handler.MauiContext);
		}

		// Handle the content mapping through the NavigationRootManager
		if (handler is WindowHandler gnomeHandler && gnomeHandler._navigationRootManager != null)
		{
			gnomeHandler._navigationRootManager.Connect(handler);
		}
	}

	public static void MapX(IWindowHandler handler, IWindow view)
	{
		// Position mapping not yet implemented for GNOME
	}

	public static void MapY(IWindowHandler handler, IWindow view)
	{
		// Position mapping not yet implemented for GNOME
	}

	public static void MapWidth(IWindowHandler handler, IWindow view)
	{
		if (handler.PlatformView is Adw.ApplicationWindow appWindow && view.Width > 0)
		{
			appWindow.DefaultWidth = (int)view.Width;
		}
	}

	public static void MapHeight(IWindowHandler handler, IWindow view)
	{
		if (handler.PlatformView is Adw.ApplicationWindow appWindow && view.Height > 0)
		{
			appWindow.DefaultHeight = (int)view.Height;
		}
	}

	public static void MapToolbar(IWindowHandler handler, IWindow view)
	{
		if (view is IToolbarElement tb)
		{
			ViewHandler.MapToolbar(handler, tb);
		}
	}

	public static void MapMenuBar(IWindowHandler handler, IWindow view)
	{
		if (handler is WindowHandler gnomeHandler &&
			gnomeHandler._navigationRootManager != null &&
			view is IMenuBarElement menuBarElement)
		{
			var menuBar = menuBarElement.MenuBar?.ToHandler(handler.MauiContext!)?.PlatformView as Gio.Menu;
			gnomeHandler._navigationRootManager.SetMenuBar(menuBar);
		}
	}

	public static void MapRequestDisplayDensity(IWindowHandler handler, IWindow window, object? args)
	{
		if (args is DisplayDensityRequest request)
		{
			// Default density - could be retrieved from GDK display scale factor
			request.SetResult(1.0f);
		}
	}
}
