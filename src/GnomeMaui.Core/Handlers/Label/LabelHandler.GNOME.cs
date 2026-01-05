namespace Microsoft.Maui.Handlers;

public partial class LabelHandler : ViewHandler<ILabel, Gtk.Label>
{
	protected override Gtk.Label CreatePlatformView()
		=> this.Create();

	public override bool NeedsContainer
		=>
			VirtualView?.Background != null
			||
			(VirtualView != null && VirtualView.VerticalTextAlignment != TextAlignment.Start)
			||
			base.NeedsContainer;

	protected override void SetupContainer()
	{
		// VerticalAlignment only works when the child's Height is Auto
		PlatformView.HeightRequest = -1;
		MapHeight(this, VirtualView);
	}

	protected override void RemoveContainer()
	{
		MapHeight(this, VirtualView);
	}

	public static void MapHeight(ILabelHandler handler, ILabel view)
		=> handler.ToPlatform().UpdateHeight(view);

	public static void MapText(ILabelHandler handler, ILabel label)
		=> handler.PlatformView?.UpdateText(label);

	public static void MapTextColor(ILabelHandler handler, ILabel label)
		=> handler.PlatformView?.UpdateTextColor(label);
	public static void MapCharacterSpacing(ILabelHandler handler, ILabel label)
		=> handler.PlatformView?.UpdateCharacterSpacing(label);
	public static void MapFont(ILabelHandler handler, ILabel label)
		=> handler.PlatformView?.UpdateFont(label);
	public static void MapHorizontalTextAlignment(ILabelHandler handler, ILabel label)
		=> handler.PlatformView?.UpdateHorizontalTextAlignment(label);
	public static void MapVerticalTextAlignment(ILabelHandler handler, ILabel label)
		=> handler.PlatformView?.UpdateVerticalTextAlignment(label);
	public static void MapTextDecorations(ILabelHandler handler, ILabel label)
		=> handler.PlatformView?.UpdateTextDecorations(label);
	public static void MapMaxLines(ILabelHandler handler, ILabel label) { }
	public static void MapPadding(ILabelHandler handler, ILabel label)
		=> handler.PlatformView?.UpdatePadding(label);
	public static void MapLineHeight(ILabelHandler handler, ILabel label)
		=> handler.PlatformView?.UpdateLineHeight(label);

}