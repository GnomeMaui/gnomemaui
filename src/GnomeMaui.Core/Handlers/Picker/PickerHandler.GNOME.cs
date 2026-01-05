namespace Microsoft.Maui.Handlers;

public partial class PickerHandler : ViewHandler<IPicker, Gtk.ComboBoxText>
{
	protected override Gtk.ComboBoxText CreatePlatformView()
		=> this.Create();

	protected override void ConnectHandler(Gtk.ComboBoxText platformView)
	{
		base.ConnectHandler(platformView);
		platformView.OnChanged += OnComboBoxChanged;
	}

	protected override void DisconnectHandler(Gtk.ComboBoxText platformView)
	{
		platformView.OnChanged -= OnComboBoxChanged;
		base.DisconnectHandler(platformView);
	}

	void OnComboBoxChanged(Gtk.ComboBox sender, EventArgs args)
	{
		if (VirtualView != null && PlatformView != null)
		{
			VirtualView.SelectedIndex = PlatformView.Active;
		}
	}

	internal static void MapItems(IPickerHandler handler, IPicker picker)
		=> handler.PlatformView?.UpdateItems(picker);

	public static void MapTitle(IPickerHandler handler, IPicker view)
		=> handler.PlatformView?.UpdateTitle(view);

	public static void MapTitleColor(IPickerHandler handler, IPicker view)
		=> handler.PlatformView?.UpdateTitleColor(view);

	public static void MapSelectedIndex(IPickerHandler handler, IPicker view)
		=> handler.PlatformView?.UpdateSelectedIndex(view);

	public static void MapCharacterSpacing(IPickerHandler handler, IPicker view)
		=> handler.PlatformView?.UpdateCharacterSpacing(view);

	public static void MapFont(IPickerHandler handler, IPicker view)
		=> handler.PlatformView?.UpdateFont(view);

	public static void MapTextColor(IPickerHandler handler, IPicker view)
		=> handler.PlatformView?.UpdateTextColor(view);

	public static void MapHorizontalTextAlignment(IPickerHandler handler, IPicker view)
		=> handler.PlatformView?.UpdateHorizontalTextAlignment(view);

	public static void MapVerticalTextAlignment(IPickerHandler handler, IPicker view)
		=> handler.PlatformView?.UpdateVerticalTextAlignment(view);

	internal static void MapIsOpen(IPickerHandler handler, IPicker picker)
		=> handler.PlatformView?.UpdateIsOpen(picker);
}