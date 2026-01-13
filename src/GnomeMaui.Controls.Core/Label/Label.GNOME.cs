using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.View"/> that displays text.</summary>
	public partial class Label
	{
		public static void MapText(ILabelHandler handler, Label label)
			=> handler.PlatformView?.UpdateText(label);
		public static void MapLineBreakMode(ILabelHandler handler, Label label)
			=> handler.PlatformView?.UpdateLineBreakMode(label);
		public static void MapMaxLines(ILabelHandler handler, Label label)
			=> handler.PlatformView?.UpdateMaxLines(label);
		public static void MapTextColor(ILabelHandler handler, Label label)
			=> handler.PlatformView?.UpdateTextColor(label);
		public static void MapCharacterSpacing(ILabelHandler handler, Label label)
			=> handler.PlatformView?.UpdateCharacterSpacing(label);
		public static void MapFont(ILabelHandler handler, Label label)
			=> handler.PlatformView?.UpdateFont(label);
		public static void MapHorizontalTextAlignment(ILabelHandler handler, Label label)
			=> handler.PlatformView?.UpdateHorizontalTextAlignment(label);
		public static void MapVerticalTextAlignment(ILabelHandler handler, Label label)
			=> handler.PlatformView?.UpdateVerticalTextAlignment(label);
		public static void MapTextDecorations(ILabelHandler handler, Label label)
			=> handler.PlatformView?.UpdateTextDecorations(label);
		public static void MapPadding(ILabelHandler handler, Label label)
			=> handler.PlatformView?.UpdatePadding(label);
		public static void MapLineHeight(ILabelHandler handler, Label label)
			=> handler.PlatformView?.UpdateLineHeight(label);
	}
}
