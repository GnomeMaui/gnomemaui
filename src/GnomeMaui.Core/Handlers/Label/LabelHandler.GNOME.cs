using Microsoft.Maui.Graphics;
using System.Text;

namespace Microsoft.Maui.Handlers;

public partial class LabelHandler : ViewHandler<ILabel, Gtk.Label>
{
	protected override Gtk.Label CreatePlatformView() => this.Create();

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

	public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
	{

		var platformView = PlatformView;
		var virtualView = VirtualView;

		if (platformView == null || virtualView == null)
		{
			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		if (widthConstraint < 0 || heightConstraint < 0)
		{
			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}
		int measuredWidth;
		int measuredHeight;

		// If the label has wrap enabled and we have a width constraint, measure with it
		if (platformView.Wrap && !double.IsInfinity(widthConstraint))
		{
			int widthInPixels = (int)Math.Ceiling(widthConstraint);

			using var pangoContext = platformView.GetPangoContext();
			using var measuringLayout = Pango.Layout.New(pangoContext);

			// Másold át a label ÖSSZES tulajdonságát és attribútumát
			var text = platformView.Label_ ?? string.Empty;
			measuringLayout.SetText(text, -1);

			// Wrap beállítások
			measuringLayout.SetWrap(platformView.WrapMode);
			measuringLayout.SetEllipsize(platformView.Ellipsize);
			measuringLayout.SetJustify(platformView.Justify == Gtk.Justification.Fill);
			var labelAttributes = platformView.GetAttributes();
			if (labelAttributes != null)
			{
				measuringLayout.SetAttributes(labelAttributes);
			}

			// Width constraint beállítása
			measuringLayout.SetWidth(widthInPixels * Pango.Constants.SCALE);

			// Mérés
			measuringLayout.GetPixelExtents(out var inkRect, out var logicalRect);
			measuredWidth = logicalRect.Width;
			measuredHeight = logicalRect.Height;
		}
		else if (platformView.Wrap && double.IsInfinity(widthConstraint))
		{
			// No width constraint: use MINIMUM size for wrapping labels
			// This allows the layout system to provide the actual constraint later
			platformView.GetPreferredSize(out var minimumSize, out var naturalSize);
			measuredWidth = minimumSize.Width;
			measuredHeight = minimumSize.Height;
		}
		else
		{
			// Non-wrapping label: use default measurement
			platformView.GetPreferredSize(out var minimumSize, out var naturalSize);
			measuredWidth = naturalSize.Width;
			measuredHeight = naturalSize.Height;
		}

		// Adjust for explicit sizes
		double? explicitWidth = (virtualView.Width >= 0) ? virtualView.Width : null;
		double? explicitHeight = (virtualView.Height >= 0) ? virtualView.Height : null;

		var width = explicitWidth ?? Math.Min(measuredWidth, double.IsInfinity(widthConstraint) ? measuredWidth : widthConstraint);
		var height = explicitHeight ?? measuredHeight;

		return new Size(width, height);
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
	public static void MapPadding(ILabelHandler handler, ILabel label)
		=> handler.PlatformView?.UpdatePadding(label);
	public static void MapLineHeight(ILabelHandler handler, ILabel label)
		=> handler.PlatformView?.UpdateLineHeight(label);

}