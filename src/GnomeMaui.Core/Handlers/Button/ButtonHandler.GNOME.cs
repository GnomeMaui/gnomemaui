using GnomeMaui.CSS;
using SkiaSharp;

namespace Microsoft.Maui.Handlers;

public partial class ButtonHandler : ViewHandler<IButton, Gtk.Button>
{
	protected override Gtk.Button CreatePlatformView()
	{
		return this.Create();
	}

	public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke)
	{
		handler.PlatformView?.UpdateStrokeColor(buttonStroke);
	}

	public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke)
	{
		handler.PlatformView?.UpdateStrokeThickness(buttonStroke);
	}

	public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke)
	{
		handler.PlatformView?.UpdateCornerRadius(buttonStroke);
	}

	public static void MapText(IButtonHandler handler, IText button)
	{
		handler.PlatformView?.UpdateText(button);
	}

	public static void MapTextColor(IButtonHandler handler, ITextStyle button)
	{
		handler.PlatformView?.UpdateTextColor(button);
	}

	public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
	{
		handler.PlatformView?.UpdateCharacterSpacing(button);
	}

	public static void MapFont(IButtonHandler handler, ITextStyle button)
	{
		handler.PlatformView?.UpdateFont(button);
	}

	public static void MapPadding(IButtonHandler handler, IButton button)
	{
		handler.PlatformView?.UpdatePadding(button);
	}

	public static void MapBackground(IButtonHandler handler, IButton button)
	{
		handler.PlatformView?.UpdateBackground(button);
	}

	public static void MapImageSource(IButtonHandler handler, IImage image) { }

	partial class ButtonImageSourcePartSetter
	{
		public override void SetImageSource(Microsoft.Maui.Platform.SKImageView? platformImage) { }
	}

	protected override void ConnectHandler(Gtk.Button platformView)
	{
		base.ConnectHandler(platformView);
		platformView.OnClicked += OnButtonClicked;
	}

	protected override void DisconnectHandler(Gtk.Button platformView)
	{
		platformView.OnClicked -= OnButtonClicked;
		base.DisconnectHandler(platformView);
	}

	void OnButtonClicked(Gtk.Button sender, EventArgs args)
	{
		VirtualView?.Clicked();
	}
}