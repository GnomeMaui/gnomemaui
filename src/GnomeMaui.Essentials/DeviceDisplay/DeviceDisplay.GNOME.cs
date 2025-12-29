using System;

namespace Microsoft.Maui.Devices;

partial class DeviceDisplayImplementation
{
	Gio.ListModel? _monitors;
	List<Gdk.Monitor>? _monitorRefs;
	uint _inhibitCookie;

	protected override bool GetKeepScreenOn()
	{
		return _inhibitCookie != 0;
	}

	protected override void SetKeepScreenOn(bool keepScreenOn)
	{
		var appHandle = Gio.Internal.Application.GetDefault();
		if (appHandle == IntPtr.Zero)
			return;

		if (keepScreenOn)
		{
			if (_inhibitCookie == 0)
			{
				var windowHandle = Gtk.Internal.Application.GetActiveWindow(appHandle);
				using var reason = GLib.Internal.NullableUtf8StringOwnedHandle.Create("Keep screen on requested by application");
				_inhibitCookie = Gtk.Internal.Application.Inhibit(
					appHandle,
					windowHandle,
					Gtk.ApplicationInhibitFlags.Idle,
					reason);
			}
		}
		else
		{
			if (_inhibitCookie != 0)
			{
				Gtk.Internal.Application.Uninhibit(appHandle, _inhibitCookie);
				_inhibitCookie = 0;
			}
		}
	}

	protected override DisplayInfo GetMainDisplayInfo()
	{
		var display = Gdk.Display.GetDefault();
		if (display is null)
			return new DisplayInfo();

		var monitors = display.GetMonitors();
		if (monitors.GetNItems() == 0)
			return new DisplayInfo();

		// Determine the default connector from the active window surface
		string? defaultConnector = null;
		var appHandle = Gio.Internal.Application.GetDefault();
		if (appHandle != IntPtr.Zero)
		{
			var windowHandle = Gtk.Internal.Application.GetActiveWindow(appHandle);
			if (windowHandle != IntPtr.Zero)
			{
				var surfacePtr = Gtk.Internal.Native.GetSurface(windowHandle);
				var monitorAtSurface = Gdk.Internal.Display.GetMonitorAtSurface(display.Handle.DangerousGetHandle(), surfacePtr);
				if (monitorAtSurface != IntPtr.Zero)
				{
					var defaultMonitor = new Gdk.Monitor(new Gdk.Internal.MonitorHandle(monitorAtSurface, false));
					defaultConnector = defaultMonitor.Connector;
				}
			}
		}

		// Find monitor matching the default connector, or use first monitor as fallback
		Gdk.Monitor? monitor = null;
		uint n = monitors.GetNItems();
		for (uint i = 0; i < n; i++)
		{
			var monitorPtr = monitors.GetItem(i);
			var m = new Gdk.Monitor(new Gdk.Internal.MonitorHandle(monitorPtr, false));
			if (m is null)
				continue;

			monitor ??= m;

			if (defaultConnector != null && m.Connector == defaultConnector)
			{
				monitor = m;
				break;
			}
		}

		if (monitor is null)
			return new DisplayInfo();

		// Calculate density from DPI
		double density = 1.0;
		if (monitor.WidthMm > 0 && monitor.HeightMm > 0)
		{
			double dpiX = monitor.Geometry.Width / (monitor.WidthMm / 25.4);
			double dpiY = monitor.Geometry.Height / (monitor.HeightMm / 25.4);
			density = (dpiX + dpiY) / (2.0 * 96.0);
		}

		var orientation = monitor.Geometry.Width > monitor.Geometry.Height
			? DisplayOrientation.Landscape
			: DisplayOrientation.Portrait;

		return new DisplayInfo(
			width: monitor.Geometry.Width,
			height: monitor.Geometry.Height,
			density: density,
			orientation: orientation,
			rotation: DisplayRotation.Rotation0,
			rate: monitor.RefreshRate / 1000.0f);
	}

	protected override void StartScreenMetricsListeners()
	{
		var display = Gdk.Display.GetDefault();
		if (display is null)
			return;

		_monitors = display.GetMonitors();
		if (_monitors is null)
			return;

		// Listen for monitor add/remove events
		_monitors.OnItemsChanged += OnMonitorsChanged;

		// Subscribe to property changes on all existing monitors
		_monitorRefs = [];
		uint n = _monitors.GetNItems();
		for (uint i = 0; i < n; i++)
		{
			var monitorPtr = _monitors.GetItem(i);
			var monitor = new Gdk.Monitor(new Gdk.Internal.MonitorHandle(monitorPtr, false));
			if (monitor is not null)
			{
				monitor.OnNotify += OnMonitorPropertyChanged;
				_monitorRefs.Add(monitor);
			}
		}
	}

	protected override void StopScreenMetricsListeners()
	{
		_monitors?.OnItemsChanged -= OnMonitorsChanged;
		_monitors = null;

		if (_monitorRefs is not null)
		{
			foreach (var monitor in _monitorRefs)
			{
				monitor.OnNotify -= OnMonitorPropertyChanged;
			}
			_monitorRefs.Clear();
			_monitorRefs = null;
		}
	}

	void OnMonitorsChanged(Gio.ListModel? sender, Gio.ListModel.ItemsChangedSignalArgs args)
	{
		OnMainDisplayInfoChanged();
	}

	void OnMonitorPropertyChanged(GObject.Object sender, GObject.Object.NotifySignalArgs args)
	{
		var propertyName = args.Pspec.GetName();
		if (propertyName == "scale" || propertyName == "geometry" || propertyName == "refresh-rate")
		{
			OnMainDisplayInfoChanged();
		}
	}
}
