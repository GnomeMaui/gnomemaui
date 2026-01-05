using GObject;

namespace Microsoft.Maui.Handlers;

public partial class SwitchHandler : ViewHandler<ISwitch, Gtk.Switch>
{
	// Keep signal handler reference to prevent GC
	private ReturningSignalHandler<Gtk.Switch, Gtk.Switch.StateSetSignalArgs, bool>? _stateSetHandler;

	protected override Gtk.Switch CreatePlatformView()
	{
		return this.Create();
	}

	protected override void ConnectHandler(Gtk.Switch platformView)
	{
		_stateSetHandler = OnStateSet;
		platformView.OnStateSet += _stateSetHandler;
		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(Gtk.Switch platformView)
	{
		if (_stateSetHandler != null)
		{
			platformView.OnStateSet -= _stateSetHandler;
			_stateSetHandler = null;
		}
		base.DisconnectHandler(platformView);
	}

	bool OnStateSet(Gtk.Switch sender, Gtk.Switch.StateSetSignalArgs args)
	{
		if (VirtualView == null)
			return false;

		var isOn = args.State;
		if (VirtualView.IsOn != isOn)
			VirtualView.IsOn = isOn;

		return false;
	}

	public static void MapIsOn(ISwitchHandler handler, ISwitch view)
	{
		if (handler is SwitchHandler switchHandler)
			switchHandler.PlatformView?.UpdateIsOn(view);
	}

	public static void MapTrackColor(ISwitchHandler handler, ISwitch view)
	{
		if (handler is SwitchHandler switchHandler)
			switchHandler.PlatformView?.UpdateTrackColor(view);
	}

	public static void MapThumbColor(ISwitchHandler handler, ISwitch view)
	{
		if (handler is SwitchHandler switchHandler)
			switchHandler.PlatformView?.UpdateThumbColor(view);
	}
}