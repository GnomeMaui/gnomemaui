using GObject;

namespace Microsoft.Maui.Handlers;

public partial class SliderHandler : ViewHandler<ISlider, Gtk.Scale>
{
	// Keep signal handler reference to prevent GC
	private SignalHandler<Gtk.Range>? _valueChangedHandler;

	protected override Gtk.Scale CreatePlatformView()
	{
		return this.Create();
	}

	protected override void ConnectHandler(Gtk.Scale platformView)
	{
		_valueChangedHandler = OnValueChanged;
		platformView.OnValueChanged += _valueChangedHandler;
		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(Gtk.Scale platformView)
	{
		if (_valueChangedHandler != null)
		{
			platformView.OnValueChanged -= _valueChangedHandler;
			_valueChangedHandler = null;
		}
		base.DisconnectHandler(platformView);
	}

	void OnValueChanged(Gtk.Range sender, EventArgs args)
	{
		if (VirtualView == null)
			return;

		var value = sender.GetValue();
		VirtualView.Value = value;
	}

	public static void MapMinimum(IViewHandler handler, ISlider slider)
	{
		if (handler is ISliderHandler sliderHandler)
			sliderHandler.PlatformView?.UpdateMinimum(slider);
	}

	public static void MapMaximum(IViewHandler handler, ISlider slider)
	{
		if (handler is ISliderHandler sliderHandler)
			sliderHandler.PlatformView?.UpdateMaximum(slider);
	}

	public static void MapValue(IViewHandler handler, ISlider slider)
	{
		if (handler is ISliderHandler sliderHandler)
			sliderHandler.PlatformView?.UpdateValue(slider);
	}

	public static void MapMinimumTrackColor(IViewHandler handler, ISlider slider)
	{
		if (handler is ISliderHandler sliderHandler)
			sliderHandler.PlatformView?.UpdateMinimumTrackColor(slider);
	}

	public static void MapMaximumTrackColor(IViewHandler handler, ISlider slider)
	{
		if (handler is ISliderHandler sliderHandler)
			sliderHandler.PlatformView?.UpdateMaximumTrackColor(slider);
	}

	public static void MapThumbColor(IViewHandler handler, ISlider slider)
	{
		if (handler is ISliderHandler sliderHandler)
			sliderHandler.PlatformView?.UpdateThumbColor(slider);
	}

	public static void MapThumbImageSource(IViewHandler handler, ISlider slider)
	{
		if (handler is ISliderHandler sliderHandler)
		{
			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
			sliderHandler.PlatformView?.UpdateThumbImageSource(slider, provider);
		}
	}
}
